namespace GestorProyect.applications.exeptions;

public sealed class RecursoNoEncontradoException : Exception
{
    public RecursoNoEncontradoException(string recurso, Guid id)
        : base($"No se encontró {recurso} con el identificador '{id}'.")
    {
    }
}
