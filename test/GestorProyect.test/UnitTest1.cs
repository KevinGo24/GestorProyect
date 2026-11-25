using GestorProyect.applications;
using GestorProyect.applications.dtos;
using GestorProyect.applications.exeptions;
using GestorProyect.models.entities;

namespace GestorProyect.test;

public class ProyectoServiceTests
{
    private ProyectoService _service = null!;

    [SetUp]
    public void Setup()
    {
        _service = new ProyectoService();
    }

    [Test]
    public void Crear_TrimmeaCamposYAgregaProyecto()
    {
        var inicio = new DateTime(2026, 1, 1);
        var finalizacion = new DateTime(2026, 2, 1);

        var proyecto = _service.Crear(new GuardarProyectoDto("  Proyecto A  ", "  Descripcion  ", inicio, finalizacion));

        Assert.Multiple(() =>
        {
            Assert.That(proyecto.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(proyecto.Nombre, Is.EqualTo("Proyecto A"));
            Assert.That(proyecto.Descripcion, Is.EqualTo("Descripcion"));
            Assert.That(proyecto.FechaInicio, Is.EqualTo(inicio));
            Assert.That(proyecto.FechaFinalizacion, Is.EqualTo(finalizacion));
            Assert.That(_service.ObtenerTodos(), Has.Count.EqualTo(1));
            Assert.That(_service.ObtenerPorId(proyecto.Id), Is.EqualTo(proyecto));
        });
    }

    [Test]
    public void Actualizar_ModificaProyectoExistente()
    {
        var proyecto = _service.Crear(CrearProyectoDto());
        var actualizado = _service.Actualizar(
            proyecto.Id,
            new GuardarProyectoDto("Nuevo nombre", "Nueva descripcion", new DateTime(2026, 3, 1), new DateTime(2026, 4, 1)));

        Assert.That(actualizado.Nombre, Is.EqualTo("Nuevo nombre"));
        Assert.That(_service.ObtenerPorId(proyecto.Id), Is.EqualTo(actualizado));
    }

    [Test]
    public void Eliminar_QuitaProyectoYNoLoEncuentra()
    {
        var proyecto = _service.Crear(CrearProyectoDto());

        _service.Eliminar(proyecto.Id);

        Assert.That(_service.ObtenerTodos(), Is.Empty);
        Assert.Throws<RecursoNoEncontradoException>(() => _service.ObtenerPorId(proyecto.Id));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Crear_ConNombreVacio_LanzaExcepcion(string nombre)
    {
        var dto = CrearProyectoDto() with { Nombre = nombre };

        Assert.Throws<ValidacionAplicacionException>(() => _service.Crear(dto));
    }

    [Test]
    public void Crear_ConFechaFinalAnteriorAInicio_LanzaExcepcion()
    {
        var dto = new GuardarProyectoDto("Proyecto", "", new DateTime(2026, 2, 1), new DateTime(2026, 1, 1));

        Assert.Throws<ValidacionAplicacionException>(() => _service.Crear(dto));
    }

    [Test]
    public void ObtenerPorId_ConProyectoInexistente_LanzaExcepcion()
    {
        Assert.Throws<RecursoNoEncontradoException>(() => _service.ObtenerPorId(Guid.NewGuid()));
    }

    private static GuardarProyectoDto CrearProyectoDto() =>
        new("Proyecto", "Descripcion", new DateTime(2026, 1, 1), new DateTime(2026, 2, 1));
}

public class TareaServiceTests
{
    private ProyectoService _proyectos = null!;
    private TareaService _service = null!;
    private Guid _proyectoId;

    [SetUp]
    public void Setup()
    {
        _proyectos = new ProyectoService();
        _proyectoId = _proyectos.Crear(
            new GuardarProyectoDto("Proyecto", "", new DateTime(2026, 1, 1), new DateTime(2026, 2, 1))).Id;
        _service = new TareaService(_proyectos);
    }

    [Test]
    public void Crear_TrimmeaCamposYAsociaLaTareaAlProyecto()
    {
        var tarea = _service.Crear(CrearTareaDto() with { Titulo = "  Tarea A  ", Prioridad = " Alta " });

        Assert.Multiple(() =>
        {
            Assert.That(tarea.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(tarea.Titulo, Is.EqualTo("Tarea A"));
            Assert.That(tarea.Prioridad, Is.EqualTo("Alta"));
            Assert.That(tarea.ProyectoId, Is.EqualTo(_proyectoId));
            Assert.That(_service.ObtenerTodos(), Has.Count.EqualTo(1));
            Assert.That(_service.ObtenerPorId(tarea.Id), Is.EqualTo(tarea));
        });
    }

    [Test]
    public void Actualizar_ModificaTareaExistente()
    {
        var tarea = _service.Crear(CrearTareaDto());
        var dto = CrearTareaDto() with
        {
            Titulo = "Tarea actualizada",
            Estado = EstadoTarea.Completada,
            Prioridad = "Baja"
        };

        var actualizado = _service.Actualizar(tarea.Id, dto);

        Assert.That(actualizado.Titulo, Is.EqualTo("Tarea actualizada"));
        Assert.That(actualizado.Estado, Is.EqualTo(EstadoTarea.Completada));
        Assert.That(actualizado.Prioridad, Is.EqualTo("Baja"));
        Assert.That(_service.ObtenerPorId(tarea.Id), Is.EqualTo(actualizado));
    }

    [Test]
    public void Eliminar_QuitaTareaYNoLaEncuentra()
    {
        var tarea = _service.Crear(CrearTareaDto());

        _service.Eliminar(tarea.Id);

        Assert.That(_service.ObtenerTodos(), Is.Empty);
        Assert.Throws<RecursoNoEncontradoException>(() => _service.ObtenerPorId(tarea.Id));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Crear_ConTituloVacio_LanzaExcepcion(string titulo)
    {
        var dto = CrearTareaDto() with { Titulo = titulo };

        Assert.Throws<ValidacionAplicacionException>(() => _service.Crear(dto));
    }

    [Test]
    public void Crear_ConPrioridadVacia_LanzaExcepcion()
    {
        var dto = CrearTareaDto() with { Prioridad = " " };

        Assert.Throws<ValidacionAplicacionException>(() => _service.Crear(dto));
    }

    [Test]
    public void Crear_ConEstadoInvalido_LanzaExcepcion()
    {
        var dto = CrearTareaDto() with { Estado = (EstadoTarea)99 };

        Assert.Throws<ValidacionAplicacionException>(() => _service.Crear(dto));
    }

    [Test]
    public void Crear_ConProyectoInexistente_LanzaExcepcion()
    {
        var dto = CrearTareaDto() with { ProyectoId = Guid.NewGuid() };

        Assert.Throws<RecursoNoEncontradoException>(() => _service.Crear(dto));
    }

    [Test]
    public void Constructor_ConServicioDeProyectosNulo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentNullException>(() => new TareaService(null!));
    }

    private GuardarTareaDto CrearTareaDto() =>
        new("Tarea", "Descripcion", EstadoTarea.Pendiente, "Media", new DateTime(2026, 1, 15), _proyectoId);
}