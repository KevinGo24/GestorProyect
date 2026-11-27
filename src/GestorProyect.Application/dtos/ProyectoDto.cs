namespace GestorProyect.applications.dtos;

public sealed record ProyectoDto(
    Guid Id,
    string Nombre,
    string Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFinalizacion);

public sealed record GuardarProyectoDto(
    string Nombre,
    string Descripcion,
    DateOnly FechaInicio,
    DateOnly FechaFinalizacion);

public sealed record AvanceProyectoDto(
    Guid ProyectoId,
    int TotalTareas,
    int TareasCompletadas,
    decimal PorcentajeAvance);
