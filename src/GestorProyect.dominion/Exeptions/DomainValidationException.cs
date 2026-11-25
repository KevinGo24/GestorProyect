using GestorProyect.models.errors;

namespace GestorProyect.models.exeptions;

public sealed class DomainValidationException : DomainException
{
    public DomainValidationException(Error error)
        : base(error)
    {
    }
}
