using GestorProyect.applications.dtos;
using GestorProyect.applications.exeptions;
using GestorProyect.applications.interfaces;
using GestorProyect.models.entities;

namespace GestorProyect.applications;

public sealed class TareaService : ITareaService
{
    private readonly ITareaRepository _tareas;
    private readonly IProyectoRepository _proyectos;

    private static readonly string[] PrioridadesPermitidas = ["Baja", "Media", "Alta"];

    public TareaService(ITareaRepository tareas, IProyectoRepository proyectos)
    {
        _tareas = tareas ?? throw new ArgumentNullException(nameof(tareas));
        _proyectos = proyectos ?? throw new ArgumentNullException(nameof(proyectos));
    }

    public async Task<IReadOnlyCollection<TareaDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var tareas = await _tareas.ObtenerTodosAsync(cancellationToken);
        return tareas.Select(Mapear).ToArray();
    }

    public async Task<IReadOnlyCollection<TareaDto>> FiltrarAsync(
        Guid? proyectoId,
        EstadoTarea? estado,
        string? prioridad,
        CancellationToken cancellationToken = default)
    {
        if (estado.HasValue && !Enum.IsDefined(estado.Value))
        {
            throw new ValidacionAplicacionException("El estado de la tarea no es válido.");
        }

        var prioridadNormalizada = NormalizarPrioridad(prioridad, permitirVacia: true);
        var tareas = await _tareas.FiltrarAsync(
            proyectoId, estado, prioridadNormalizada, cancellationToken);
        return tareas.Select(Mapear).ToArray();
    }

    public async Task<IReadOnlyCollection<TareaDto>> ObtenerPorProyectoAsync(
        Guid proyectoId,
        CancellationToken cancellationToken = default)
    {
        await EncontrarProyectoAsync(proyectoId, cancellationToken);
        var tareas = await _tareas.FiltrarAsync(proyectoId, null, null, cancellationToken);
        return tareas.Select(Mapear).ToArray();
    }

    public async Task<IReadOnlyCollection<TareaDto>> ObtenerVencidasNoCompletadasAsync(
        CancellationToken cancellationToken = default)
    {
        var tareas = await _tareas.ObtenerVencidasNoCompletadasAsync(
            DateOnly.FromDateTime(DateTime.Today), cancellationToken);
        return tareas.Select(Mapear).ToArray();
    }

    public async Task<TareaDto> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Mapear(await EncontrarAsync(id, cancellationToken));

    public async Task<TareaDto> CrearAsync(GuardarTareaDto dto, CancellationToken cancellationToken = default)
    {
        await ValidarAsync(dto, cancellationToken);
        var tarea = new Tarea
        {
            Id = Guid.NewGuid(),
            Titulo = dto.Titulo.Trim(),
            Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
            Estado = dto.Estado,
            Prioridad = NormalizarPrioridad(dto.Prioridad)!,
            FechaLimite = dto.FechaLimite,
            ProyectoId = dto.ProyectoId
        };

        await _tareas.AgregarAsync(tarea, cancellationToken);
        return Mapear(tarea);
    }

    public async Task<TareaDto> ActualizarAsync(
        Guid id,
        GuardarTareaDto dto,
        CancellationToken cancellationToken = default)
    {
        await ValidarAsync(dto, cancellationToken);
        var tarea = await EncontrarAsync(id, cancellationToken);
        tarea.Titulo = dto.Titulo.Trim();
        tarea.Descripcion = dto.Descripcion?.Trim() ?? string.Empty;
        tarea.Estado = dto.Estado;
        tarea.Prioridad = NormalizarPrioridad(dto.Prioridad)!;
        tarea.FechaLimite = dto.FechaLimite;
        tarea.ProyectoId = dto.ProyectoId;
        await _tareas.GuardarCambiosAsync(cancellationToken);
        return Mapear(tarea);
    }

    public async Task<TareaDto> CambiarEstadoAsync(
        Guid id,
        EstadoTarea estado,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(estado))
        {
            throw new ValidacionAplicacionException("El estado de la tarea no es válido.");
        }

        var tarea = await EncontrarAsync(id, cancellationToken);
        tarea.Estado = estado;
        await _tareas.GuardarCambiosAsync(cancellationToken);
        return Mapear(tarea);
    }

    public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tarea = await EncontrarAsync(id, cancellationToken);
        await _tareas.EliminarAsync(tarea, cancellationToken);
    }

    private async Task<Tarea> EncontrarAsync(Guid id, CancellationToken cancellationToken) =>
        await _tareas.ObtenerPorIdAsync(id, cancellationToken) is { } tarea
            ? tarea
            : throw new RecursoNoEncontradoException("la tarea", id);

    private async Task ValidarAsync(GuardarTareaDto dto, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.Titulo))
        {
            throw new ValidacionAplicacionException("El título de la tarea es obligatorio.");
        }

        _ = NormalizarPrioridad(dto.Prioridad);

        if (!Enum.IsDefined(dto.Estado))
        {
            throw new ValidacionAplicacionException("El estado de la tarea no es válido.");
        }

        await EncontrarProyectoAsync(dto.ProyectoId, cancellationToken);
    }

    private async Task EncontrarProyectoAsync(Guid id, CancellationToken cancellationToken)
    {
        _ = await _proyectos.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException("el proyecto", id);
    }

    private static string? NormalizarPrioridad(string? prioridad, bool permitirVacia = false)
    {
        if (string.IsNullOrWhiteSpace(prioridad))
        {
            if (permitirVacia)
            {
                return null;
            }

            throw new ValidacionAplicacionException("La prioridad de la tarea es obligatoria.");
        }

        var normalizada = PrioridadesPermitidas.FirstOrDefault(
            permitida => string.Equals(permitida, prioridad.Trim(), StringComparison.OrdinalIgnoreCase));
        return normalizada
            ?? throw new ValidacionAplicacionException(
                $"La prioridad debe ser una de estas opciones: {string.Join(", ", PrioridadesPermitidas)}.");
    }

    private static TareaDto Mapear(Tarea tarea) =>
        new(tarea.Id, tarea.Titulo, tarea.Descripcion, tarea.Estado, tarea.Prioridad, tarea.FechaLimite, tarea.ProyectoId);
}
