using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace GestorProyect.Api.UnitTest;

[TestFixture]
public sealed class ApiOpenApiTests
{
    [Test]
    public async Task OpenApi_PublicaRutasV1ConResumenYDescripcion()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureAppConfiguration((_, configuration) =>
                    configuration.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] =
                            "Host=localhost;Database=swagger_test;Username=test;Password=test"
                    }));
            });
        using var client = factory.CreateClient();

        using var document = JsonDocument.Parse(await client.GetStringAsync("/openapi/v1.json"));
        var paths = document.RootElement.GetProperty("paths");
        foreach (var path in paths.EnumerateObject())
        {
            Assert.That(path.Name, Does.StartWith("/api/v1/"));
            foreach (var operation in path.Value.EnumerateObject())
            {
                Assert.That(operation.Value.GetProperty("summary").GetString(), Is.Not.Empty,
                    $"{operation.Name} {path.Name} debe tener un resumen.");
                Assert.That(operation.Value.GetProperty("description").GetString(), Is.Not.Empty,
                    $"{operation.Name} {path.Name} debe tener una descripción.");
            }
        }

        var getProjects = paths.GetProperty("/api/v1/proyectos").GetProperty("get");
        var createTask = paths.GetProperty("/api/v1/tareas").GetProperty("post");
        var taskStatus = paths.GetProperty("/api/v1/tareas/{id}/estado").GetProperty("patch");

        Assert.That(getProjects.GetProperty("summary").GetString(), Is.EqualTo("Lista todos los proyectos"));
        Assert.That(getProjects.GetProperty("description").GetString(), Does.Contain("proyectos registrados"));
        Assert.That(createTask.GetProperty("summary").GetString(), Is.EqualTo("Crea una tarea"));
        Assert.That(taskStatus.GetProperty("summary").GetString(), Is.EqualTo("Cambia el estado de una tarea"));
    }
}
