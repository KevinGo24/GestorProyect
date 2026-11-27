namespace GestorProyect.models.errors;

public static class TareaErrors
{
    public static readonly Error TituloRequerido =
        new("Tarea.TituloRequerido", "El título de la tarea es obligatorio.");

    public static readonly Error ProyectoIdRequerido =
        new("Tarea.ProyectoIdRequerido", "La tarea debe estar asociada a un proyecto.");

    public static readonly Error PrioridadRequerida =
        new("Tarea.PrioridadRequerida", "La prioridad de la tarea es obligatoria.");
}
