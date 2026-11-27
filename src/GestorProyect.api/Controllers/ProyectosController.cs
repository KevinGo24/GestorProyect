using GestorProyect.applications.dtos;
using GestorProyect.applications.exeptions;
using GestorProyect.applications.interfaces;
using GestorProyect.models.entities;
using Microsoft.AspNetCore.Mvc;

namespace GestorProyect.api.Controllers;

[ApiController]
[Route("api/v1/proyectos")]
[Produces("application/json")]
public sealed class ProyectosController(IProyectoService proyectos, ITareaService tareas) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Lista todos los proyectos")]
    [EndpointDescription("Devuelve los proyectos registrados con sus datos y fechas.")]
    [ProducesResponseType<IReadOnlyCollection<ProyectoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ProyectoDto>>> ObtenerTodos(
        CancellationToken cancellationToken) =>
        Ok(await proyectos.ObtenerTodosAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [EndpointSummary("Obtiene un proyecto")]
    [EndpointDescription("Busca un proyecto por su identificador. Devuelve 404 si no existe.")]
    [ProducesResponseType<ProyectoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProyectoDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await proyectos.ObtenerPorIdAsync(id, cancellationToken));
        }
        catch (RecursoNoEncontradoException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [HttpGet("{id:guid}/tareas")]
    [EndpointSummary("Lista las tareas de un proyecto")]
    [EndpointDescription("Devuelve todas las tareas asociadas al proyecto indicado. Devuelve 404 si el proyecto no existe.")]
    [ProducesResponseType<IReadOnlyCollection<TareaDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyCollection<TareaDto>>> ObtenerTareas(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await tareas.ObtenerPorProyectoAsync(id, cancellationToken));
        }
        catch (RecursoNoEncontradoException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [HttpGet("{id:guid}/avance")]
    [EndpointSummary("Consulta el avance de un proyecto")]
    [EndpointDescription("Calcula el total de tareas, las completadas y el porcentaje de avance. Devuelve 404 si el proyecto no existe.")]
    [ProducesResponseType<AvanceProyectoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AvanceProyectoDto>> ObtenerAvance(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await proyectos.ObtenerAvanceAsync(id, cancellationToken));
        }
        catch (RecursoNoEncontradoException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [HttpPost]
    [EndpointSummary("Crea un proyecto")]
    [EndpointDescription("Registra un proyecto nuevo. El nombre es obligatorio y la fecha final no puede ser anterior a la fecha de inicio.")]
    [ProducesResponseType<ProyectoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProyectoDto>> Crear(
        [FromBody] GuardarProyectoDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var proyecto = await proyectos.CrearAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = proyecto.Id }, proyecto);
        }
        catch (ValidacionAplicacionException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [EndpointSummary("Actualiza un proyecto")]
    [EndpointDescription("Reemplaza los datos del proyecto indicado. Devuelve 404 si no existe o 400 si los datos no son válidos.")]
    [ProducesResponseType<ProyectoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProyectoDto>> Actualizar(
        Guid id,
        [FromBody] GuardarProyectoDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await proyectos.ActualizarAsync(id, dto, cancellationToken));
        }
        catch (RecursoNoEncontradoException exception)
        {
            return NotFound(exception.Message);
        }
        catch (ValidacionAplicacionException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [EndpointSummary("Elimina un proyecto")]
    [EndpointDescription("Elimina el proyecto indicado si no tiene tareas pendientes. Devuelve 404 si no existe y 400 si no se puede eliminar.")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await proyectos.EliminarAsync(id, cancellationToken);
            return NoContent();
        }
        catch (RecursoNoEncontradoException exception)
        {
            return NotFound(exception.Message);
        }
        catch (ValidacionAplicacionException exception)
        {
            return BadRequest(exception.Message);
        }
    }
}
