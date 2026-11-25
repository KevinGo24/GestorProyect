using System.Collections.Concurrent;
using GestorProyect.applications.dtos;
using GestorProyect.applications.exeptions;
using GestorProyect.applications.interfaces;
using GestorProyect.models.entities;

namespace GestorProyect.applications;

public sealed class ProyectoService : IProyectoService
{
    private readonly ConcurrentDictionary<Guid, Proyecto> _proyectos = new();

    public IReadOnlyCollection<ProyectoDto> ObtenerTodos() =>
        _proyectos.Values.Select(Mapear).ToArray();

    public ProyectoDto ObtenerPorId(Guid id) =>
        Mapear(Encontrar(id));

    public ProyectoDto Crear(GuardarProyectoDto dto)
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

        _proyectos[proyecto.Id] = proyecto;
        return Mapear(proyecto);
    }

    public ProyectoDto Actualizar(Guid id, GuardarProyectoDto dto)
    {
        Validar(dto);
        var proyecto = Encontrar(id);
        proyecto.Nombre = dto.Nombre.Trim();
        proyecto.Descripcion = dto.Descripcion?.Trim() ?? string.Empty;
        proyecto.FechaInicio = dto.FechaInicio;
        proyecto.FechaFinalizacion = dto.FechaFinalizacion;
        return Mapear(proyecto);
    }

    public void Eliminar(Guid id)
    {
        if (!_proyectos.TryRemove(id, out _))
        {
            throw new RecursoNoEncontradoException("el proyecto", id);
        }
    }

    private Proyecto Encontrar(Guid id) =>
        _proyectos.TryGetValue(id, out var proyecto)
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
