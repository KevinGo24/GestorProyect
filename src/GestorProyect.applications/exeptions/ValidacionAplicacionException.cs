namespace GestorProyect.applications.exeptions;

public sealed class ValidacionAplicacionException : Exception
{
    public ValidacionAplicacionException(string message)
        : base(message)
    {
    }
}
