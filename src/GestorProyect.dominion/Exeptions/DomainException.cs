using GestorProyect.models.errors;

namespace GestorProyect.models.exeptions;

public class DomainException : Exception
{
    public Error Error { get; }

    public DomainException(Error error)
        : base(error.Description)
    {
        Error = error;
    }
}
