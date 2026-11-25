using GestorProyect.models.errors;
using GestorProyect.models.exeptions;

namespace GestorProyect.models.valueobject;

public sealed record Prioridad
{
    public string Valor { get; }

    private Prioridad(string valor)
    {
        Valor = valor;
    }

    public static Prioridad Crear(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new DomainValidationException(TareaErrors.PrioridadRequerida);
        }

        return new Prioridad(valor.Trim());
    }
}
