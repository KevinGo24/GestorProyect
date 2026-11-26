# GestorProyect

API REST para administrar proyectos y sus tareas. El sistema está desarrollado en .NET 10, organizado por capas y utiliza PostgreSQL como base de datos.

## Meta del proyecto

Entregar un backend que permita gestionar el ciclo de vida de proyectos y tareas, consultar su avance y aplicar reglas de negocio para fechas, estados y eliminación segura de proyectos.

## Logros implementados

- CRUD de proyectos y tareas.
- Consulta de tareas asociadas a un proyecto.
- Filtros de tareas por proyecto, estado y prioridad.
- Consulta de tareas vencidas que todavía no están completadas.
- Cambio específico del estado de una tarea mediante `PATCH`.
- Cálculo del porcentaje de avance a partir de tareas completadas; un proyecto sin tareas informa `0 %`.
- Bloqueo de eliminación de proyectos que tengan tareas pendientes.
- Validaciones para nombres, títulos, prioridades, estados, fechas y referencias a proyectos.
- Respuestas HTTP para recursos no encontrados y solicitudes inválidas, además de manejo global de errores.
- Persistencia con Entity Framework Core y PostgreSQL, migración inicial y documentación OpenAPI/Swagger.
- Pruebas unitarias para servicios y pruebas de integración configuradas para ejecutar la API contra PostgreSQL.

## API

La API documenta sus endpoints en Swagger UI bajo `/swagger` cuando se ejecuta en Development.

| Funcionalidad | Método y ruta |
|---|---|
| CRUD de proyectos | `/api/proyectos` |
| Tareas de un proyecto | `GET /api/proyectos/{id}/tareas` |
| Avance del proyecto | `GET /api/proyectos/{id}/avance` |
| CRUD de tareas | `/api/tareas` |
| Filtrar tareas | `GET /api/tareas?proyectoId={guid}&estado=Pendiente&prioridad=Alta` |
| Tareas vencidas no completadas | `GET /api/tareas/vencidas` |
| Cambiar estado | `PATCH /api/tareas/{id}/estado` |

El cambio de estado recibe, por ejemplo, `{"estado":"Completada"}`. Los estados permitidos son `Pendiente`, `EnProgreso` y `Completada`; las prioridades permitidas son `Baja`, `Media` y `Alta`. Las fechas se reciben y devuelven en formato `dd-MM-yy`.

## Arquitectura

```text
src/
  GestorProyect.api/          API HTTP, controladores y configuración
  GestorProyect.applications/ Servicios, DTOs, interfaces y reglas de aplicación
  GestorProyect.dominion/     Entidades y conceptos del dominio
  GestorProyect.Infratuctur/  EF Core, PostgreSQL y repositorios
test/
  GestorProyect.test/         Pruebas unitarias y de integración
```

## Configuración y ejecución

Configura `ConnectionStrings__DefaultConnection` con la conexión a PostgreSQL. No guardes contraseñas en archivos versionados.

```bash
dotnet build gestorproyect.slnx
dotnet run --project src/GestorProyect.api/GestorProyect.api.csproj
```

Para aplicar las migraciones a la base configurada:

```bash
dotnet ef database update \
  --project src/GestorProyect.Infratuctur/GestorProyect.Infratuctur.csproj \
  --startup-project src/GestorProyect.api/GestorProyect.api.csproj
```

## Pruebas y estado

Ejecuta la suite con:

```bash
dotnet test test/GestorProyect.test/GestorProyect.test.csproj
```

Las pruebas unitarias verificadas actualmente pasan. Las pruebas de integración levantan la API en memoria y requieren PostgreSQL; hasta configurar una base de pruebas exclusiva, NUnit las omite. Para ejecutarlas, configura `GESTORPROYECT_TEST_CONNECTION` con la cadena de conexión de esa base. La suite crea un esquema temporal, aplica las migraciones y lo elimina al finalizar; el usuario necesita permisos para crear y eliminar esquemas. **No uses una base de producción para estas pruebas.**

## Próximas metas

- Ejecutar las pruebas de integración contra una base PostgreSQL de pruebas y registrar el resultado.
- Configurar PostgreSQL de Azure con reglas de red seguras y conexión cifrada.
- Automatizar compilación y pruebas en CI.
