using GestorProyect.Infratuctur.Persistence;
using GestorProyect.Infratuctur.Repository;
using GestorProyect.models.entities;
using Microsoft.EntityFrameworkCore;

namespace GestorProyect.Infrastructure.UnitTest;

public sealed class RepositoryTests
{
    [Fact]
    public async Task ProyectoRepository_PersisteConsultaYEliminaProyecto()
    {
        await using var db = CrearDbContext();
        var repository = new ProyectoRepository(db);
        var proyecto = CrearProyecto();

        await repository.AgregarAsync(proyecto);
        var recuperado = await repository.ObtenerPorIdAsync(proyecto.Id);
        Assert.Equal(proyecto.Nombre, recuperado?.Nombre);
        Assert.Single(await repository.ObtenerTodosAsync());

        await repository.EliminarAsync(recuperado!);
        Assert.Empty(await repository.ObtenerTodosAsync());
    }

    [Fact]
    public async Task TareaRepository_FiltraPorCriteriosYDetectaPendientes()
    {
        await using var db = CrearDbContext();
        var repository = new TareaRepository(db);
        var proyectoId = Guid.NewGuid();
        await db.Proyectos.AddAsync(CrearProyecto(proyectoId));
        var pendiente = new Tarea
        {
            Id = Guid.NewGuid(),
            ProyectoId = proyectoId,
            Estado = EstadoTarea.Pendiente,
            Prioridad = "Alta",
            FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(-1))
        };
        await repository.AgregarAsync(pendiente);
        await repository.AgregarAsync(new Tarea
        {
            Id = Guid.NewGuid(),
            ProyectoId = proyectoId,
            Estado = EstadoTarea.Completada,
            Prioridad = "Alta",
            FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(-2))
        });

        var filtradas = await repository.FiltrarAsync(proyectoId, EstadoTarea.Pendiente, "Alta");
        var vencidas = await repository.ObtenerVencidasNoCompletadasAsync(DateOnly.FromDateTime(DateTime.Today));

        Assert.Single(filtradas);
        Assert.Equal(pendiente.Id, filtradas.Single().Id);
        Assert.Single(vencidas);
        Assert.True(await repository.TieneTareasPendientesAsync(proyectoId));
        Assert.Equal(2, await repository.ContarPorProyectoAsync(proyectoId));
        Assert.Equal(1, await repository.ContarPorEstadoAsync(proyectoId, EstadoTarea.Completada));
    }

    private static GestorProyectDbContext CrearDbContext()
    {
        var options = new DbContextOptionsBuilder<GestorProyectDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new GestorProyectDbContext(options);
    }

    private static Proyecto CrearProyecto(Guid? id = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Nombre = "Proyecto de prueba",
            Descripcion = "",
            FechaInicio = new DateOnly(2026, 1, 1),
            FechaFinalizacion = new DateOnly(2026, 2, 1)
        };
}
