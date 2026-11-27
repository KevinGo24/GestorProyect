namespace GestorProyect.models.errors;

public static class ProyectoErrors
{
    public static readonly Error NombreRequerido =
        new("Proyecto.NombreRequerido", "El nombre del proyecto es obligatorio.");

    public static readonly Error RangoFechasInvalido =
        new("Proyecto.RangoFechasInvalido", "La fecha de finalización no puede ser anterior a la fecha de inicio.");
}
