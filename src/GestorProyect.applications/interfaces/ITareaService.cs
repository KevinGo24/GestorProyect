using GestorProyect.applications.dtos;
using GestorProyect.models.entities;

namespace GestorProyect.applications.interfaces;

public interface ITareaService
{
    Task<IReadOnlyCollection<TareaDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TareaDto>> FiltrarAsync(
        Guid? proyectoId,
        EstadoTarea? estado,
        string? prioridad,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TareaDto>> ObtenerPorProyectoAsync(
        Guid proyectoId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TareaDto>> ObtenerVencidasNoCompletadasAsync(
        CancellationToken cancellationToken = default);
    Task<TareaDto> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TareaDto> CrearAsync(GuardarTareaDto dto, CancellationToken cancellationToken = default);
    Task<TareaDto> ActualizarAsync(Guid id, GuardarTareaDto dto, CancellationToken cancellationToken = default);
    Task<TareaDto> CambiarEstadoAsync(
        Guid id,
        EstadoTarea estado,
        CancellationToken cancellationToken = default);
    Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);
}
