using GestorProyect.applications.interfaces;
using GestorProyect.Infratuctur.Persistence;
using GestorProyect.models.entities;
using Microsoft.EntityFrameworkCore;

namespace GestorProyect.Infratuctur.Repository;

public sealed class ProyectoRepository(GestorProyectDbContext dbContext) : IProyectoRepository
{
    public async Task<IReadOnlyCollection<Proyecto>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default) =>
        await dbContext.Proyectos.AsNoTracking().ToListAsync(cancellationToken);

    public Task<Proyecto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Proyectos.FirstOrDefaultAsync(proyecto => proyecto.Id == id, cancellationToken);

    public async Task AgregarAsync(Proyecto proyecto, CancellationToken cancellationToken = default)
    {
        await dbContext.Proyectos.AddAsync(proyecto, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);

    public async Task EliminarAsync(Proyecto proyecto, CancellationToken cancellationToken = default)
    {
        dbContext.Proyectos.Remove(proyecto);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
