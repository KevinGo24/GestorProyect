using System.Collections.Concurrent;
using GestorProyect.applications.dtos;
using GestorProyect.applications.exeptions;
using GestorProyect.applications.interfaces;
using GestorProyect.models.entities;

namespace GestorProyect.applications;

public sealed class TareaService : ITareaService
{
    private readonly ConcurrentDictionary<Guid, Tarea> _tareas = new();
    private readonly IProyectoService _proyectos;

    public TareaService(IProyectoService proyectos)
    {
        _proyectos = proyectos ?? throw new ArgumentNullException(nameof(proyectos));
    }

    public IReadOnlyCollection<TareaDto> ObtenerTodos() =>
        _tareas.Values.Select(Mapear).ToArray();

    public TareaDto ObtenerPorId(Guid id) =>
        Mapear(Encontrar(id));

    public TareaDto Crear(GuardarTareaDto dto)
    {
        Validar(dto);
        var tarea = new Tarea
        {
            Id = Guid.NewGuid(),
            Titulo = dto.Titulo.Trim(),
            Descripcion = dto.Descripcion?.Trim() ?? string.Empty,
            Estado = dto.Estado,
            Prioridad = dto.Prioridad.Trim(),
            FechaLimite = dto.FechaLimite,
            ProyectoId = dto.ProyectoId
        };

        _tareas[tarea.Id] = tarea;
        return Mapear(tarea);
    }

    public TareaDto Actualizar(Guid id, GuardarTareaDto dto)
    {
        Validar(dto);
        var tarea = Encontrar(id);
        tarea.Titulo = dto.Titulo.Trim();
        tarea.Descripcion = dto.Descripcion?.Trim() ?? string.Empty;
        tarea.Estado = dto.Estado;
        tarea.Prioridad = dto.Prioridad.Trim();
        tarea.FechaLimite = dto.FechaLimite;
        tarea.ProyectoId = dto.ProyectoId;
        return Mapear(tarea);
    }

    public void Eliminar(Guid id)
    {
        if (!_tareas.TryRemove(id, out _))
        {
            throw new RecursoNoEncontradoException("la tarea", id);
        }
    }

    private Tarea Encontrar(Guid id) =>
        _tareas.TryGetValue(id, out var tarea)
            ? tarea
            : throw new RecursoNoEncontradoException("la tarea", id);

    private void Validar(GuardarTareaDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        if (string.IsNullOrWhiteSpace(dto.Titulo))
        {
            throw new ValidacionAplicacionException("El título de la tarea es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(dto.Prioridad))
        {
            throw new ValidacionAplicacionException("La prioridad de la tarea es obligatoria.");
        }

        if (!Enum.IsDefined(dto.Estado))
        {
            throw new ValidacionAplicacionException("El estado de la tarea no es válido.");
        }

        _ = _proyectos.ObtenerPorId(dto.ProyectoId);
    }

    private static TareaDto Mapear(Tarea tarea) =>
        new(tarea.Id, tarea.Titulo, tarea.Descripcion, tarea.Estado, tarea.Prioridad, tarea.FechaLimite, tarea.ProyectoId);
}
