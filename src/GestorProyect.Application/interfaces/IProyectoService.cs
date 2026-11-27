using GestorProyect.applications.dtos;

namespace GestorProyect.applications.interfaces;

public interface IProyectoService
{
    Task<IReadOnlyCollection<ProyectoDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<ProyectoDto> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProyectoDto> CrearAsync(GuardarProyectoDto dto, CancellationToken cancellationToken = default);
    Task<ProyectoDto> ActualizarAsync(Guid id, GuardarProyectoDto dto, CancellationToken cancellationToken = default);
    Task<AvanceProyectoDto> ObtenerAvanceAsync(Guid id, CancellationToken cancellationToken = default);
    Task EliminarAsync(Guid id, CancellationToken cancellationToken = default);
}
