using GestorProyect.applications.dtos;
using GestorProyect.applications.exeptions;
using GestorProyect.applications.interfaces;
using GestorProyect.models.entities;
using Microsoft.AspNetCore.Mvc;

namespace GestorProyect.api.Controllers;

[ApiController]
[Route("api/proyectos")]
[Produces("application/json")]
public sealed class ProyectosController(IProyectoService proyectos, ITareaService tareas) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<ProyectoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ProyectoDto>>> ObtenerTodos(
        CancellationToken cancellationToken) =>
        Ok(await proyectos.ObtenerTodosAsync(cancellationToken));

    [HttpGet("{id:guid}")]
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
