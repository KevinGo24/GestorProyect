using GestorProyect.models.entities;

namespace GestorProyect.applications.interfaces;

public interface ITareaRepository
{
    Task<IReadOnlyCollection<Tarea>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Tarea>> FiltrarAsync(
        Guid? proyectoId,
        EstadoTarea? estado,
        string? prioridad,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Tarea>> ObtenerVencidasNoCompletadasAsync(
        DateOnly fecha,
        CancellationToken cancellationToken = default);
    Task<bool> TieneTareasPendientesAsync(Guid proyectoId, CancellationToken cancellationToken = default);
    Task<int> ContarPorProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default);
    Task<int> ContarPorEstadoAsync(
        Guid proyectoId,
        EstadoTarea estado,
        CancellationToken cancellationToken = default);
    Task<Tarea?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AgregarAsync(Tarea tarea, CancellationToken cancellationToken = default);
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
    Task EliminarAsync(Tarea tarea, CancellationToken cancellationToken = default);
}
