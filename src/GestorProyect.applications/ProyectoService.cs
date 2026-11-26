using GestorProyect.applications.dtos;
using GestorProyect.applications.exeptions;
using GestorProyect.applications.interfaces;
using GestorProyect.models.entities;

namespace GestorProyect.applications;

public sealed class ProyectoService : IProyectoService
{
    private readonly IProyectoRepository _proyectos;
    private readonly ITareaRepository _tareas;

    public ProyectoService(IProyectoRepository proyectos, ITareaRepository tareas)
    {
        _proyectos = proyectos ?? throw new ArgumentNullException(nameof(proyectos));
        _tareas = tareas ?? throw new ArgumentNullException(nameof(tareas));
    }

    public async Task<IReadOnlyCollection<ProyectoDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var proyectos = await _proyectos.ObtenerTodosAsync(cancellationToken);
        return proyectos.Select(Mapear).ToArray();
    }

    public async Task<ProyectoDto> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Mapear(await EncontrarAsync(id, cancellationToken));

    public async Task<ProyectoDto> CrearAsync(
        GuardarProyectoDto dto,
        CancellationToken cancellationToken = default)
    {
        Validar(dto);
        var proyecto = new Proyecto
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
            FechaInicio = dto.FechaInicio,
            FechaFinalizacion = dto.FechaFinalizacion
        };

        await _proyectos.AgregarAsync(proyecto, cancellationToken);
        return Mapear(proyecto);
    }

    public async Task<ProyectoDto> ActualizarAsync(
        Guid id,
        GuardarProyectoDto dto,
        CancellationToken cancellationToken = default)
    {
        Validar(dto);
        var proyecto = await EncontrarAsync(id, cancellationToken);
        proyecto.Nombre = dto.Nombre.Trim();
        proyecto.Descripcion = dto.Descripcion?.Trim() ?? string.Empty;
        proyecto.FechaInicio = dto.FechaInicio;
        proyecto.FechaFinalizacion = dto.FechaFinalizacion;
        await _proyectos.GuardarCambiosAsync(cancellationToken);
        return Mapear(proyecto);
    }

    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var proyecto = await EncontrarAsync(id, cancellationToken);
        if (await _tareas.TieneTareasPendientesAsync(id, cancellationToken))
        {
            throw new ValidacionAplicacionException(
                "No se puede eliminar el proyecto porque tiene tareas pendientes.");
        }

        await _proyectos.EliminarAsync(proyecto, cancellationToken);
    }

    public async Task<AvanceProyectoDto> ObtenerAvanceAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _ = await EncontrarAsync(id, cancellationToken);
        var total = await _tareas.ContarPorProyectoAsync(id, cancellationToken);
        var completadas = await _tareas.ContarPorEstadoAsync(id, EstadoTarea.Completada, cancellationToken);
        var porcentaje = total == 0 ? 0m : decimal.Round(completadas * 100m / total, 2);

        return new AvanceProyectoDto(id, total, completadas, porcentaje);
    }

    private async Task<Proyecto> EncontrarAsync(Guid id, CancellationToken cancellationToken) =>
        await _proyectos.ObtenerPorIdAsync(id, cancellationToken) is { } proyecto
            ? proyecto
            : throw new RecursoNoEncontradoException("el proyecto", id);

    private static void Validar(GuardarProyectoDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            throw new ValidacionAplicacionException("El nombre del proyecto es obligatorio.");
        }

        if (dto.FechaFinalizacion < dto.FechaInicio)
        {
            throw new ValidacionAplicacionException(
                "La fecha de finalización no puede ser anterior a la fecha de inicio.");
        }
    }

    private static ProyectoDto Mapear(Proyecto proyecto) =>
        new(proyecto.Id, proyecto.Nombre, proyecto.Descripcion, proyecto.FechaInicio, proyecto.FechaFinalizacion);
}
