using GestorProyect.models.entities;

namespace GestorProyect.applications.dtos;

public sealed record TareaDto(
    Guid Id,
    string Titulo,
    string Descripcion,
    EstadoTarea Estado,
    string Prioridad,
    DateTime FechaLimite,
    Guid ProyectoId);

public sealed record GuardarTareaDto(
    string Titulo,
    string Descripcion,
    EstadoTarea Estado,
    string Prioridad,
    DateTime FechaLimite,
    Guid ProyectoId);
