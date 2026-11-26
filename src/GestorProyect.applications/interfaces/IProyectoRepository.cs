using GestorProyect.models.entities;

namespace GestorProyect.applications.interfaces;

public interface IProyectoRepository
{
    Task<IReadOnlyCollection<Proyecto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<Proyecto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AgregarAsync(Proyecto proyecto, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
    Task EliminarAsync(Proyecto proyecto, CancellationToken cancellationToken = default);
}
