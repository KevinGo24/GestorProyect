using GestorProyect.applications;
using GestorProyect.applications.dtos;
using GestorProyect.applications.exeptions;
using GestorProyect.applications.interfaces;
using GestorProyect.models.entities;

namespace GestorProyect.Applications.Unitest;

public sealed class ProyectoYTareaServiceTests
{
    [Fact]
    public async Task CrearProyecto_NormalizaCamposYCalculaAvance()
    {
        var proyectos = new ProyectoRepositoryFake();
        var tareas = new TareaRepositoryFake();
        var servicio = new ProyectoService(proyectos, tareas);
        var dto = new GuardarProyectoDto(
            "  Proyecto de prueba  ",
            "  Descripción  ",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 2, 1));

        var proyecto = await servicio.CrearAsync(dto);
        tareas.Items.Add(new Tarea
        {
            Id = Guid.NewGuid(),
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.Completada
        });
        tareas.Items.Add(new Tarea
        {
            Id = Guid.NewGuid(),
            ProyectoId = proyecto.Id,
            Estado = EstadoTarea.Pendiente
        });

        var avance = await servicio.ObtenerAvanceAsync(proyecto.Id);

        Assert.Equal("Proyecto de prueba", proyecto.Nombre);
        Assert.Equal("Descripción", proyecto.Descripcion);
        Assert.Equal(2, avance.TotalTareas);
        Assert.Equal(1, avance.TareasCompletadas);
        Assert.Equal(50m, avance.PorcentajeAvance);
    }

    [Fact]
    public async Task EliminarProyectoConTareasPendientes_LanzaValidacion()
    {
        var proyectos = new ProyectoRepositoryFake();
        var tareas = new TareaRepositoryFake();
        var servicio = new ProyectoService(proyectos, tareas);
        var proyecto = await servicio.CrearAsync(CrearProyectoDto());
        tareas.Items.Add(new Tarea { ProyectoId = proyecto.Id, Estado = EstadoTarea.Pendiente });

        await Assert.ThrowsAsync<ValidacionAplicacionException>(() => servicio.EliminarAsync(proyecto.Id));
        Assert.NotNull(await proyectos.ObtenerPorIdAsync(proyecto.Id));
    }

    [Fact]
    public async Task CrearTarea_NormalizaPrioridadYRechazaProyectoInexistente()
    {
        var proyectos = new ProyectoRepositoryFake();
        var tareas = new TareaRepositoryFake();
        var servicio = new TareaService(tareas, proyectos);
        var proyecto = await proyectos.AgregarProyectoAsync();
        var dto = new GuardarTareaDto(
            "  Tarea  ",
            "  Descripción  ",
            EstadoTarea.Pendiente,
            " alta ",
            new DateOnly(2026, 1, 15),
            proyecto.Id);

        var tarea = await servicio.CrearAsync(dto);
        Assert.Equal("Tarea", tarea.Titulo);
        Assert.Equal("Descripción", tarea.Descripcion);
        Assert.Equal("Alta", tarea.Prioridad);

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() => servicio.CrearAsync(
            dto with { ProyectoId = Guid.NewGuid() }));
    }

    [Fact]
    public async Task FiltrarTareas_RechazaPrioridadNoPermitida()
    {
        var servicio = new TareaService(new TareaRepositoryFake(), new ProyectoRepositoryFake());

        await Assert.ThrowsAsync<ValidacionAplicacionException>(
            () => servicio.FiltrarAsync(null, null, "Urgente"));
    }

    private static GuardarProyectoDto CrearProyectoDto() =>
        new("Proyecto", "", new DateOnly(2026, 1, 1), new DateOnly(2026, 2, 1));

    private sealed class ProyectoRepositoryFake : IProyectoRepository
    {
        private readonly Dictionary<Guid, Proyecto> _items = [];

        public Task<IReadOnlyCollection<Proyecto>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Proyecto>>(_items.Values.ToArray());

        public Task<Proyecto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_items.GetValueOrDefault(id));

        public Task AgregarAsync(Proyecto proyecto, CancellationToken cancellationToken = default)
        {
            _items.Add(proyecto.Id, proyecto);
            return Task.CompletedTask;
        }

        public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task EliminarAsync(Proyecto proyecto, CancellationToken cancellationToken = default)
        {
            _items.Remove(proyecto.Id);
            return Task.CompletedTask;
        }

        public async Task<Proyecto> AgregarProyectoAsync()
        {
            var proyecto = new Proyecto
            {
                Id = Guid.NewGuid(),
                Nombre = "Proyecto",
                FechaInicio = new DateOnly(2026, 1, 1),
                FechaFinalizacion = new DateOnly(2026, 2, 1)
            };
            await AgregarAsync(proyecto);
            return proyecto;
        }
    }

    private sealed class TareaRepositoryFake : ITareaRepository
    {
        public List<Tarea> Items { get; } = [];

        public Task<IReadOnlyCollection<Tarea>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Tarea>>(Items.ToArray());

        public Task<IReadOnlyCollection<Tarea>> FiltrarAsync(
            Guid? proyectoId,
            EstadoTarea? estado,
            string? prioridad,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Tarea>>(Items
                .Where(item => !proyectoId.HasValue || item.ProyectoId == proyectoId)
                .Where(item => !estado.HasValue || item.Estado == estado)
                .Where(item => prioridad is null || item.Prioridad == prioridad)
                .ToArray());

        public Task<IReadOnlyCollection<Tarea>> ObtenerVencidasNoCompletadasAsync(
            DateOnly fecha,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyCollection<Tarea>>(Items
                .Where(item => item.FechaLimite < fecha && item.Estado != EstadoTarea.Completada)
                .ToArray());

        public Task<bool> TieneTareasPendientesAsync(Guid proyectoId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.Any(item =>
                item.ProyectoId == proyectoId && item.Estado == EstadoTarea.Pendiente));

        public Task<int> ContarPorProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.Count(item => item.ProyectoId == proyectoId));

        public Task<int> ContarPorEstadoAsync(
            Guid proyectoId,
            EstadoTarea estado,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.Count(item =>
                item.ProyectoId == proyectoId && item.Estado == estado));

        public Task<Tarea?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(Items.FirstOrDefault(item => item.Id == id));

        public Task AgregarAsync(Tarea tarea, CancellationToken cancellationToken = default)
        {
            Items.Add(tarea);
            return Task.CompletedTask;
        }

        public Task GuardarCambiosAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task EliminarAsync(Tarea tarea, CancellationToken cancellationToken = default)
        {
            Items.Remove(tarea);
            return Task.CompletedTask;
        }
    }
}
