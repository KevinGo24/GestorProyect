using GestorProyect.applications;
using GestorProyect.applications.dtos;
using GestorProyect.applications.exeptions;
using GestorProyect.applications.interfaces;
using GestorProyect.models.entities;

namespace GestorProyect.test;

public class ProyectoServiceTests
{
    private ProyectoService _service = null!;
    private FakeTareaRepository _tareas = null!;

    [SetUp]
    public void Setup()
    {
        _tareas = new FakeTareaRepository();
        _service = new ProyectoService(new FakeProyectoRepository(), _tareas);
    }

    [Test]
    public async Task Crear_TrimmeaCamposYAgregaProyecto()
    {
        var inicio = new DateOnly(2026, 1, 1);
        var finalizacion = new DateOnly(2026, 2, 1);

        var proyecto = await _service.CrearAsync(
            new GuardarProyectoDto("  Proyecto A  ", "  Descripcion  ", inicio, finalizacion));

        Assert.Multiple(() =>
        {
            Assert.That(proyecto.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(proyecto.Nombre, Is.EqualTo("Proyecto A"));
            Assert.That(proyecto.Descripcion, Is.EqualTo("Descripcion"));
            Assert.That(proyecto.FechaInicio, Is.EqualTo(inicio));
            Assert.That(proyecto.FechaFinalizacion, Is.EqualTo(finalizacion));
        });
        Assert.That(await _service.ObtenerTodosAsync(), Has.Count.EqualTo(1));
        Assert.That(await _service.ObtenerPorIdAsync(proyecto.Id), Is.EqualTo(proyecto));
    }

    [Test]
    public async Task Actualizar_ModificaProyectoExistente()
    {
        var proyecto = await _service.CrearAsync(CrearProyectoDto());
        var actualizado = await _service.ActualizarAsync(
            proyecto.Id,
            new GuardarProyectoDto("Nuevo nombre", "Nueva descripcion", new DateOnly(2026, 3, 1), new DateOnly(2026, 4, 1)));

        Assert.That(actualizado.Nombre, Is.EqualTo("Nuevo nombre"));
        Assert.That(await _service.ObtenerPorIdAsync(proyecto.Id), Is.EqualTo(actualizado));
    }

    [Test]
    public async Task Eliminar_QuitaProyectoYNoLoEncuentra()
    {
        var proyecto = await _service.CrearAsync(CrearProyectoDto());

        await _service.EliminarAsync(proyecto.Id);

        Assert.That(await _service.ObtenerTodosAsync(), Is.Empty);
        Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _service.ObtenerPorIdAsync(proyecto.Id));
    }

    [Test]
    public async Task Eliminar_ConTareasPendientes_LanzaExcepcionYConservaProyecto()
    {
        var proyecto = await _service.CrearAsync(CrearProyectoDto());
        await _tareas.AgregarAsync(new Tarea
        {
            Id = Guid.NewGuid(),
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.Pendiente
        });

        Assert.ThrowsAsync<ValidacionAplicacionException>(() => _service.EliminarAsync(proyecto.Id));
        Assert.That(await _service.ObtenerPorIdAsync(proyecto.Id), Is.EqualTo(proyecto));
    }

    [Test]
    public async Task ObtenerAvance_CalculaPorcentajeDeTareasCompletadas()
    {
        var proyecto = await _service.CrearAsync(CrearProyectoDto());
        await _tareas.AgregarAsync(new Tarea { Id = Guid.NewGuid(), ProyectoId = proyecto.Id });
        await _tareas.AgregarAsync(new Tarea
        {
            Id = Guid.NewGuid(),
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.Completada
        });

        var avance = await _service.ObtenerAvanceAsync(proyecto.Id);

        Assert.Multiple(() =>
        {
            Assert.That(avance.TotalTareas, Is.EqualTo(2));
            Assert.That(avance.TareasCompletadas, Is.EqualTo(1));
            Assert.That(avance.PorcentajeAvance, Is.EqualTo(50m));
        });
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Crear_ConNombreVacio_LanzaExcepcion(string nombre)
    {
        var dto = CrearProyectoDto() with { Nombre = nombre };

        Assert.ThrowsAsync<ValidacionAplicacionException>(() => _service.CrearAsync(dto));
    }

    [Test]
    public void Crear_ConFechaFinalAnteriorAInicio_LanzaExcepcion()
    {
        var dto = new GuardarProyectoDto("Proyecto", "", new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1));

        Assert.ThrowsAsync<ValidacionAplicacionException>(() => _service.CrearAsync(dto));
    }

    [Test]
    public void ObtenerPorId_ConProyectoInexistente_LanzaExcepcion()
    {
        Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _service.ObtenerPorIdAsync(Guid.NewGuid()));
    }

    private static GuardarProyectoDto CrearProyectoDto() =>
        new("Proyecto", "Descripcion", new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1));
}

public class TareaServiceTests
{
    private FakeProyectoRepository _proyectos = null!;
    private FakeTareaRepository _tareas = null!;
    private TareaService _service = null!;
    private Guid _proyectoId;

    [SetUp]
    public async Task Setup()
    {
        _proyectos = new FakeProyectoRepository();
        var proyecto = new Proyecto
        {
            Id = Guid.NewGuid(),
            Nombre = "Proyecto",
            FechaInicio = new DateOnly(2026, 1, 1),
            FechaFinalizacion = new DateOnly(2026, 2, 1)
        };
        await _proyectos.AgregarAsync(proyecto);
        _proyectoId = proyecto.Id;
        _tareas = new FakeTareaRepository();
        _service = new TareaService(_tareas, _proyectos);
    }

    [Test]
    public async Task Crear_TrimmeaCamposYAsociaLaTareaAlProyecto()
    {
        var tarea = await _service.CrearAsync(CrearTareaDto() with { Titulo = "  Tarea A  ", Prioridad = " Alta " });

        Assert.Multiple(() =>
        {
            Assert.That(tarea.Id, Is.Not.EqualTo(Guid.Empty));
            Assert.That(tarea.Titulo, Is.EqualTo("Tarea A"));
            Assert.That(tarea.Prioridad, Is.EqualTo("Alta"));
            Assert.That(tarea.ProyectoId, Is.EqualTo(_proyectoId));
        });
        Assert.That(await _service.ObtenerTodosAsync(), Has.Count.EqualTo(1));
        Assert.That(await _service.ObtenerPorIdAsync(tarea.Id), Is.EqualTo(tarea));
    }

    [Test]
    public async Task Actualizar_ModificaTareaExistente()
    {
        var tarea = await _service.CrearAsync(CrearTareaDto());
        var dto = CrearTareaDto() with
        {
            Titulo = "Tarea actualizada",
            Estado = EstadoTarea.Completada,
            Prioridad = "Baja"
        };

        var actualizado = await _service.ActualizarAsync(tarea.Id, dto);

        Assert.That(actualizado.Titulo, Is.EqualTo("Tarea actualizada"));
        Assert.That(actualizado.Estado, Is.EqualTo(EstadoTarea.Completada));
        Assert.That(actualizado.Prioridad, Is.EqualTo("Baja"));
        Assert.That(await _service.ObtenerPorIdAsync(tarea.Id), Is.EqualTo(actualizado));
    }

    [Test]
    public async Task Eliminar_QuitaTareaYNoLaEncuentra()
    {
        var tarea = await _service.CrearAsync(CrearTareaDto());

        await _service.EliminarAsync(tarea.Id);

        Assert.That(await _service.ObtenerTodosAsync(), Is.Empty);
        Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _service.ObtenerPorIdAsync(tarea.Id));
    }

    [Test]
    public async Task Filtrar_PorProyectoEstadoYPrioridad_DevuelveSoloCoincidencias()
    {
        await _service.CrearAsync(CrearTareaDto());
        await _service.CrearAsync(CrearTareaDto() with
        {
            Estado = EstadoTarea.Completada,
            Prioridad = "Alta"
        });

        var tareas = await _service.FiltrarAsync(_proyectoId, EstadoTarea.Pendiente, "media");

        Assert.That(tareas, Has.Count.EqualTo(1));
        Assert.That(tareas.Single().Estado, Is.EqualTo(EstadoTarea.Pendiente));
    }

    [Test]
    public async Task ObtenerVencidas_ExcluyeTareasCompletadas()
    {
        var vencida = CrearTareaDto() with
        {
            FechaLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(-1))
        };
        await _service.CrearAsync(vencida);
        await _service.CrearAsync(vencida with { Estado = EstadoTarea.Completada });

        var tareas = await _service.ObtenerVencidasNoCompletadasAsync();

        Assert.That(tareas, Has.Count.EqualTo(1));
        Assert.That(tareas.Single().Estado, Is.EqualTo(EstadoTarea.Pendiente));
    }

    [Test]
    public async Task CambiarEstado_ActualizaSoloEstado()
    {
        var tarea = await _service.CrearAsync(CrearTareaDto());

        var actualizada = await _service.CambiarEstadoAsync(tarea.Id, EstadoTarea.EnProgreso);

        Assert.That(actualizada.Estado, Is.EqualTo(EstadoTarea.EnProgreso));
        Assert.That(actualizada.Titulo, Is.EqualTo(tarea.Titulo));
    }

    [TestCase("")]
    [TestCase("   ")]
    public void Crear_ConTituloVacio_LanzaExcepcion(string titulo)
    {
        var dto = CrearTareaDto() with { Titulo = titulo };

        Assert.ThrowsAsync<ValidacionAplicacionException>(() => _service.CrearAsync(dto));
    }

    [Test]
    public void Crear_ConPrioridadVacia_LanzaExcepcion()
    {
        var dto = CrearTareaDto() with { Prioridad = " " };

        Assert.ThrowsAsync<ValidacionAplicacionException>(() => _service.CrearAsync(dto));
    }

    [Test]
    public void Crear_ConPrioridadDesconocida_LanzaExcepcion()
    {
        var dto = CrearTareaDto() with { Prioridad = "Urgente" };

        Assert.ThrowsAsync<ValidacionAplicacionException>(() => _service.CrearAsync(dto));
    }

    [Test]
    public void Crear_ConEstadoInvalido_LanzaExcepcion()
    {
        var dto = CrearTareaDto() with { Estado = (EstadoTarea)99 };

        Assert.ThrowsAsync<ValidacionAplicacionException>(() => _service.CrearAsync(dto));
    }

    [Test]
    public void Crear_ConProyectoInexistente_LanzaExcepcion()
    {
        var dto = CrearTareaDto() with { ProyectoId = Guid.NewGuid() };

        Assert.ThrowsAsync<RecursoNoEncontradoException>(() => _service.CrearAsync(dto));
    }

    [Test]
    public void Constructor_ConServicioDeProyectosNulo_LanzaExcepcion()
    {
        Assert.Throws<ArgumentNullException>(() => new TareaService(null!, _proyectos));
    }

    private GuardarTareaDto CrearTareaDto() =>
        new("Tarea", "Descripcion", EstadoTarea.Pendiente, "Media", new DateOnly(2026, 1, 15), _proyectoId);
}

internal sealed class FakeProyectoRepository : IProyectoRepository
{
    private readonly Dictionary<Guid, Proyecto> _proyectos = [];

    public Task<IReadOnlyCollection<Proyecto>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Proyecto>>(_proyectos.Values.ToArray());

    public Task<Proyecto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_proyectos.GetValueOrDefault(id));

    public Task AgregarAsync(Proyecto proyecto, CancellationToken cancellationToken = default)
    {
        _proyectos.Add(proyecto.Id, proyecto);
        return Task.CompletedTask;
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task EliminarAsync(Proyecto proyecto, CancellationToken cancellationToken = default)
    {
        _proyectos.Remove(proyecto.Id);
        return Task.CompletedTask;
    }
}

internal sealed class FakeTareaRepository : ITareaRepository
{
    private readonly Dictionary<Guid, Tarea> _tareas = [];

    public Task<IReadOnlyCollection<Tarea>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Tarea>>(_tareas.Values.ToArray());

    public Task<IReadOnlyCollection<Tarea>> FiltrarAsync(
        Guid? proyectoId,
        EstadoTarea? estado,
        string? prioridad,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Tarea>>(_tareas.Values
            .Where(tarea => !proyectoId.HasValue || tarea.ProyectoId == proyectoId.Value)
            .Where(tarea => !estado.HasValue || tarea.Estado == estado.Value)
            .Where(tarea => prioridad is null || tarea.Prioridad == prioridad)
            .ToArray());

    public Task<IReadOnlyCollection<Tarea>> ObtenerVencidasNoCompletadasAsync(
        DateOnly fecha,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyCollection<Tarea>>(_tareas.Values
            .Where(tarea => tarea.FechaLimite < fecha && tarea.Estado != EstadoTarea.Completada)
            .ToArray());

    public Task<bool> TieneTareasPendientesAsync(
        Guid proyectoId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_tareas.Values.Any(tarea =>
            tarea.ProyectoId == proyectoId && tarea.Estado == EstadoTarea.Pendiente));

    public Task<int> ContarPorProyectoAsync(
        Guid proyectoId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_tareas.Values.Count(tarea => tarea.ProyectoId == proyectoId));

    public Task<int> ContarPorEstadoAsync(
        Guid proyectoId,
        EstadoTarea estado,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_tareas.Values.Count(tarea =>
            tarea.ProyectoId == proyectoId && tarea.Estado == estado));

    public Task<Tarea?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_tareas.GetValueOrDefault(id));

    public Task AgregarAsync(Tarea tarea, CancellationToken cancellationToken = default)
    {
        _tareas.Add(tarea.Id, tarea);
        return Task.CompletedTask;
    }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task EliminarAsync(Tarea tarea, CancellationToken cancellationToken = default)
    {
        _tareas.Remove(tarea.Id);
        return Task.CompletedTask;
    }
}