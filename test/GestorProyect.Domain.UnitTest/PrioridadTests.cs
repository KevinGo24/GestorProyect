using GestorProyect.models.errors;
using GestorProyect.models.exeptions;
using GestorProyect.models.valueobject;

namespace GestorProyect.Domain.UnitTest;

public sealed class PrioridadTests
{
    [Theory]
    [InlineData("Alta", "Alta")]
    [InlineData("  Media  ", "Media")]
    public void Crear_RecortaEspacios(string valor, string esperado)
    {
        var prioridad = Prioridad.Crear(valor);

        Assert.Equal(esperado, prioridad.Valor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinValor_LanzaErrorDeDominio(string? valor)
    {
        var excepcion = Assert.Throws<DomainValidationException>(() => Prioridad.Crear(valor));

        Assert.Equal(TareaErrors.PrioridadRequerida, excepcion.Error);
    }
}
