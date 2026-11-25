namespace GestorProyect.applications.dtos;

public sealed record ProyectoDto(
    Guid Id,
    string Nombre,
    string Descripcion,
    DateTime FechaInicio,
    DateTime FechaFinalizacion);

public sealed record GuardarProyectoDto(
    string Nombre,
    string Descripcion,
    DateTime FechaInicio,
    DateTime FechaFinalizacion);
