using GestorProyect.applications.dtos;
using GestorProyect.applications.exeptions;
using GestorProyect.applications.interfaces;
using GestorProyect.models.entities;
using Microsoft.AspNetCore.Mvc;

namespace GestorProyect.api.Controllers;

[ApiController]
[Route("api/v1/tareas")]
[Produces("application/json")]
public sealed class TareasController(ITareaService tareas) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Lista y filtra tareas")]
    [EndpointDescription("Devuelve tareas y acepta filtros opcionales por proyectoId, estado y prioridad.")]
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
    [EndpointSummary("Lista tareas vencidas")]
    [EndpointDescription("Devuelve tareas cuya fecha límite ya pasó y que todavía no están completadas.")]
    [ProducesResponseType<IReadOnlyCollection<TareaDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<TareaDto>>> ObtenerVencidas(
        CancellationToken cancellationToken) =>
        Ok(await tareas.ObtenerVencidasNoCompletadasAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [EndpointSummary("Obtiene una tarea")]
    [EndpointDescription("Busca una tarea por su identificador. Devuelve 404 si no existe.")]
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
    [EndpointSummary("Crea una tarea")]
    [EndpointDescription("Registra una tarea asociada a un proyecto existente. Valida título, estado y prioridad.")]
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
    [EndpointSummary("Actualiza una tarea")]
    [EndpointDescription("Reemplaza los datos de la tarea indicada. Devuelve 404 si no existe o el proyecto no existe, y 400 si los datos no son válidos.")]
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
    [EndpointSummary("Cambia el estado de una tarea")]
    [EndpointDescription("Actualiza únicamente el estado de la tarea. Los valores válidos son Pendiente, EnProgreso y Completada.")]
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
    [EndpointSummary("Elimina una tarea")]
    [EndpointDescription("Elimina la tarea indicada. Devuelve 404 si no existe.")]
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
