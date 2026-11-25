using GestorProyect.applications.dtos;

namespace GestorProyect.applications.interfaces;

public interface IProyectoService
{
    IReadOnlyCollection<ProyectoDto> ObtenerTodos();
    ProyectoDto ObtenerPorId(Guid id);
    ProyectoDto Crear(GuardarProyectoDto dto);
    ProyectoDto Actualizar(Guid id, GuardarProyectoDto dto);
    void Eliminar(Guid id);
}
