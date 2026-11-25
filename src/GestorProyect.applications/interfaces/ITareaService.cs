using GestorProyect.applications.dtos;

namespace GestorProyect.applications.interfaces;

public interface ITareaService
{
    IReadOnlyCollection<TareaDto> ObtenerTodos();
    TareaDto ObtenerPorId(Guid id);
    TareaDto Crear(GuardarTareaDto dto);
    TareaDto Actualizar(Guid id, GuardarTareaDto dto);
    void Eliminar(Guid id);
}
