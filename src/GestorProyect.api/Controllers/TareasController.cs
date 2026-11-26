using GestorProyect.applications.dtos;
using GestorProyect.applications.exeptions;
using GestorProyect.applications.interfaces;
using GestorProyect.models.entities;
using Microsoft.AspNetCore.Mvc;

namespace GestorProyect.api.Controllers;

[ApiController]
[Route("api/tareas")]
[Produces("application/json")]
public sealed class TareasController(ITareaService tareas) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<TareaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<TareaDto>>> ObtenerTodos(
        [FromQuery] Guid? proyectoId,
        [FromQuery] EstadoTarea? estado,
        [FromQuery] string? prioridad,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await tareas.FiltrarAsync(proyectoId, estado, prioridad, cancellationToken));
        }
        catch (ValidacionAplicacionException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    [HttpGet("vencidas")]
    [ProducesResponseType<IReadOnlyCollection<TareaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<TareaDto>>> ObtenerVencidas(
        CancellationToken cancellationToken) =>
        Ok(await tareas.ObtenerVencidasNoCompletadasAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TareaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TareaDto>> ObtenerPorId(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await tareas.ObtenerPorIdAsync(id, cancellationToken));
        }
        catch (RecursoNoEncontradoException exception)
        {
            return NotFound(exception.Message);
        }
    }

    [HttpPost]
    [ProducesResponseType<TareaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TareaDto>> Crear(
        [FromBody] GuardarTareaDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            var tarea = await tareas.CrearAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = tarea.Id }, tarea);
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

    [HttpPut("{id:guid}")]
    [ProducesResponseType<TareaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TareaDto>> Actualizar(
        Guid id,
        [FromBody] GuardarTareaDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await tareas.ActualizarAsync(id, dto, cancellationToken));
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

    [HttpPatch("{id:guid}/estado")]
    [ProducesResponseType<TareaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<string>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<string>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TareaDto>> CambiarEstado(
        Guid id,
        [FromBody] CambiarEstadoTareaDto dto,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await tareas.CambiarEstadoAsync(id, dto.Estado, cancellationToken));
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
            await tareas.EliminarAsync(id, cancellationToken);
            return NoContent();
        }
        catch (RecursoNoEncontradoException exception)
        {
            return NotFound(exception.Message);
        }
    }
}
