using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using GestorProyect.Infratuctur.Persistence;

namespace GestorProyect.test;

[TestFixture]
[Category("Integration")]
public sealed class ApiIntegrationTests
{
    private string? _connectionString;
    private string? _schema;
    private WebApplicationFactory<Program>? _factory;
    private HttpClient? _client;

    [OneTimeSetUp]
    public async Task StartApi()
    {
        _connectionString = Environment.GetEnvironmentVariable("GESTORPROYECT_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            Assert.Ignore(
                "Define GESTORPROYECT_TEST_CONNECTION con una base PostgreSQL de pruebas para ejecutar integración.");
        }

        _schema = $"integration_{Guid.NewGuid():N}";
        await using (var connection = new NpgsqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE SCHEMA \"{_schema}\"", connection);
            await command.ExecuteNonQueryAsync();
        }

        var isolatedConnectionString = new NpgsqlConnectionStringBuilder(_connectionString)
        {
            SearchPath = _schema
        }.ConnectionString;

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((_, configuration) =>
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = isolatedConnectionString
                    }));
            });
        _client = _factory.CreateClient();

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GestorProyectDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    [OneTimeTearDown]
    public async Task StopApi()
    {
        _client?.Dispose();
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        if (_connectionString is not null && _schema is not null)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"DROP SCHEMA \"{_schema}\" CASCADE", connection);
            await command.ExecuteNonQueryAsync();
        }
    }

    [Test]
    public async Task Api_PermiteCrearConsultarActualizarYEliminarProyectoYTarea()
    {
        var projectId = await CrearProyectoAsync();
        var taskResponse = await Client.PostAsJsonAsync("/api/tareas", new
        {
            titulo = "Integración API",
            descripcion = "CRUD",
            estado = "Pendiente",
            prioridad = "Media",
            fechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(2)).ToString("dd-MM-yy"),
            proyectoId = projectId
        });

        Assert.That(taskResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        using var createdTask = JsonDocument.Parse(await taskResponse.Content.ReadAsStringAsync());
        var taskId = createdTask.RootElement.GetProperty("id").GetGuid();

        Assert.That((await Client.GetAsync($"/api/proyectos/{projectId}")).StatusCode,
            Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await Client.GetAsync($"/api/tareas/{taskId}")).StatusCode,
            Is.EqualTo(HttpStatusCode.OK));

        var updateResponse = await Client.PutAsJsonAsync($"/api/tareas/{taskId}", new
        {
            titulo = "Tarea actualizada",
            descripcion = "CRUD",
            estado = "EnProgreso",
            prioridad = "Alta",
            fechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(3)).ToString("dd-MM-yy"),
            proyectoId = projectId
        });
        Assert.That(updateResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await Client.DeleteAsync($"/api/tareas/{taskId}")).StatusCode,
            Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That((await Client.DeleteAsync($"/api/proyectos/{projectId}")).StatusCode,
            Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Api_FiltraTareasListaVencidasYCambiaEstado()
    {
        var projectId = await CrearProyectoAsync();
        var overdueTaskId = await CrearTareaAsync(projectId, "Pendiente", "Alta",
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)));
        var futureTaskId = await CrearTareaAsync(projectId, "Pendiente", "Baja",
            DateOnly.FromDateTime(DateTime.Today.AddDays(2)));
        var completedTaskId = await CrearTareaAsync(projectId, "Completada", "Alta",
            DateOnly.FromDateTime(DateTime.Today.AddDays(-1)));

        using var projectTasks = JsonDocument.Parse(
            await Client.GetStringAsync($"/api/proyectos/{projectId}/tareas"));
        Assert.That(projectTasks.RootElement.GetArrayLength(), Is.EqualTo(3));

        using var filtered = JsonDocument.Parse(await Client.GetStringAsync(
            $"/api/tareas?proyectoId={projectId}&estado=Pendiente&prioridad=Alta"));
        Assert.That(filtered.RootElement.GetArrayLength(), Is.EqualTo(1));
        Assert.That(filtered.RootElement[0].GetProperty("id").GetGuid(), Is.EqualTo(overdueTaskId));

        using var overdue = JsonDocument.Parse(await Client.GetStringAsync("/api/tareas/vencidas"));
        var overdueIds = overdue.RootElement.EnumerateArray()
            .Select(task => task.GetProperty("id").GetGuid()).ToArray();
        Assert.That(overdueIds, Does.Contain(overdueTaskId));
        Assert.That(overdueIds, Does.Not.Contain(completedTaskId));
        Assert.That(overdueIds, Does.Not.Contain(futureTaskId));

        var changeState = await Client.PatchAsJsonAsync(
            $"/api/tareas/{overdueTaskId}/estado", new { estado = "Completada" });
        Assert.That(changeState.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var overdueAfterUpdate = JsonDocument.Parse(await Client.GetStringAsync("/api/tareas/vencidas"));
        Assert.That(overdueAfterUpdate.RootElement.EnumerateArray()
            .Select(task => task.GetProperty("id").GetGuid()), Does.Not.Contain(overdueTaskId));

        Assert.That((await Client.DeleteAsync($"/api/proyectos/{projectId}")).StatusCode,
            Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task Api_CalculaAvanceEImpideEliminarProyectoConPendientes()
    {
        var projectId = await CrearProyectoAsync();
        var pendingTaskId = await CrearTareaAsync(projectId, "Pendiente", "Media",
            DateOnly.FromDateTime(DateTime.Today.AddDays(2)));
        await CrearTareaAsync(projectId, "EnProgreso", "Media",
            DateOnly.FromDateTime(DateTime.Today.AddDays(2)));
        await CrearTareaAsync(projectId, "Completada", "Media",
            DateOnly.FromDateTime(DateTime.Today.AddDays(2)));

        using var progress = JsonDocument.Parse(
            await Client.GetStringAsync($"/api/proyectos/{projectId}/avance"));
        Assert.That(progress.RootElement.GetProperty("totalTareas").GetInt32(), Is.EqualTo(3));
        Assert.That(progress.RootElement.GetProperty("tareasCompletadas").GetInt32(), Is.EqualTo(1));
        Assert.That(progress.RootElement.GetProperty("porcentajeAvance").GetDecimal(), Is.EqualTo(33.33m));

        var blockedDelete = await Client.DeleteAsync($"/api/proyectos/{projectId}");
        Assert.That(blockedDelete.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var changeState = await Client.PatchAsJsonAsync(
            $"/api/tareas/{pendingTaskId}/estado", new { estado = "Completada" });
        Assert.That(changeState.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That((await Client.DeleteAsync($"/api/proyectos/{projectId}")).StatusCode,
            Is.EqualTo(HttpStatusCode.NoContent));
    }

    private HttpClient Client =>
        _client ?? throw new InvalidOperationException("La API de integración no se inicializó.");

    private async Task<Guid> CrearProyectoAsync()
    {
        var response = await Client.PostAsJsonAsync("/api/proyectos", new
        {
            nombre = $"Proyecto {Guid.NewGuid():N}",
            descripcion = "Prueba de integración",
            fechaInicio = DateOnly.FromDateTime(DateTime.Today).ToString("dd-MM-yy"),
            fechaFinalizacion = DateOnly.FromDateTime(DateTime.Today.AddDays(30)).ToString("dd-MM-yy")
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("id").GetGuid();
    }

    private async Task<Guid> CrearTareaAsync(Guid projectId, string state, string priority, DateOnly dueDate)
    {
        var response = await Client.PostAsJsonAsync("/api/tareas", new
        {
            titulo = $"Tarea {Guid.NewGuid():N}",
            descripcion = "Prueba de integración",
            estado = state,
            prioridad = priority,
            fechaLimite = dueDate.ToString("dd-MM-yy"),
            proyectoId = projectId
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("id").GetGuid();
    }
}
