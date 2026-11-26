using GestorProyect.applications.interfaces;
using GestorProyect.Infratuctur.Persistence;
using GestorProyect.models.entities;
using Microsoft.EntityFrameworkCore;

namespace GestorProyect.Infratuctur.Repository;

public sealed class TareaRepository(GestorProyectDbContext dbContext) : ITareaRepository
{
    public async Task<IReadOnlyCollection<Tarea>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default) =>
        await dbContext.Tareas.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Tarea>> FiltrarAsync(
        Guid? proyectoId,
        EstadoTarea? estado,
        string? prioridad,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Tareas.AsNoTracking().AsQueryable();
        if (proyectoId.HasValue)
        {
            query = query.Where(tarea => tarea.ProyectoId == proyectoId.Value);
        }

        if (estado.HasValue)
        {
            query = query.Where(tarea => tarea.Estado == estado.Value);
        }

        if (prioridad is not null)
        {
            query = query.Where(tarea => tarea.Prioridad == prioridad);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Tarea>> ObtenerVencidasNoCompletadasAsync(
        DateOnly fecha,
        CancellationToken cancellationToken = default) =>
        await dbContext.Tareas.AsNoTracking()
            .Where(tarea => tarea.FechaLimite < fecha && tarea.Estado != EstadoTarea.Completada)
            .ToListAsync(cancellationToken);

    public Task<bool> TieneTareasPendientesAsync(
        Guid proyectoId,
        CancellationToken cancellationToken = default) =>
        dbContext.Tareas.AnyAsync(
            tarea => tarea.ProyectoId == proyectoId && tarea.Estado == EstadoTarea.Pendiente,
            cancellationToken);

    public Task<int> ContarPorProyectoAsync(
        Guid proyectoId,
        CancellationToken cancellationToken = default) =>
        dbContext.Tareas.CountAsync(tarea => tarea.ProyectoId == proyectoId, cancellationToken);

    public Task<int> ContarPorEstadoAsync(
        Guid proyectoId,
        EstadoTarea estado,
        CancellationToken cancellationToken = default) =>
        dbContext.Tareas.CountAsync(
            tarea => tarea.ProyectoId == proyectoId && tarea.Estado == estado,
            cancellationToken);

    public Task<Tarea?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Tareas.FirstOrDefaultAsync(tarea => tarea.Id == id, cancellationToken);

    public async Task AgregarAsync(Tarea tarea, CancellationToken cancellationToken = default)
    {
        await dbContext.Tareas.AddAsync(tarea, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task EliminarAsync(Tarea tarea, CancellationToken cancellationToken = default)
    {
        dbContext.Tareas.Remove(tarea);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
