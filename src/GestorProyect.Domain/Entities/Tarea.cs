namespace GestorProyect.models.entities;

public class Tarea
{
    public Guid Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public EstadoTarea Estado { get; set; } = EstadoTarea.Pendiente;
    public string Prioridad { get; set; } = string.Empty;
    public DateOnly FechaLimite { get; set; }
    public Guid ProyectoId { get; set; }
}
