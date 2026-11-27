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
- Pruebas unitarias para API, aplicación, dominio e infraestructura, además de pruebas de integración configuradas para ejecutar la API contra PostgreSQL.

## API

La API documenta sus endpoints en Swagger UI bajo `/swagger` cuando se ejecuta en Development.

| Funcionalidad | Método y ruta |
|---|---|
| CRUD de proyectos | `/api/v1/proyectos` |
| Tareas de un proyecto | `GET /api/v1/proyectos/{id}/tareas` |
| Avance del proyecto | `GET /api/v1/proyectos/{id}/avance` |
| CRUD de tareas | `/api/v1/tareas` |
| Filtrar tareas | `GET /api/v1/tareas?proyectoId={guid}&estado=Pendiente&prioridad=Alta` |
| Tareas vencidas no completadas | `GET /api/v1/tareas/vencidas` |
| Cambiar estado | `PATCH /api/v1/tareas/{id}/estado` |

El cambio de estado recibe, por ejemplo, `{"estado":"Completada"}`. Los estados permitidos son `Pendiente`, `EnProgreso` y `Completada`; las prioridades permitidas son `Baja`, `Media` y `Alta`. Las fechas se reciben y devuelven en formato `dd-MM-yy`.

## Arquitectura

```text
src/
  GestorProyect.api/          API HTTP, controladores y configuración
  GestorProyect.Application/  Servicios, DTOs, interfaces y reglas de aplicación
  GestorProyect.Domain/       Entidades y conceptos del dominio
  GestorProyect.Infratucture/ EF Core, PostgreSQL y repositorios
test/
  GestorProyect.Api.UnitTest/
  GestorProyect.Applications.Unitest/
  GestorProyect.Domain.UnitTest/
  GestorProyect.Infrastructure.UnitTest/
  GestorProyect.test/         Pruebas de integración
```

## Configuración y ejecución

La API se conecta a PostgreSQL mediante la variable de entorno `ConnectionStrings__DefaultConnection`. Para desarrollo local, puedes guardarla en un archivo `.env` en la raíz (el `.env` está ignorado por Git):

```env
ConnectionStrings__DefaultConnection='Host=<servidor>.postgres.database.azure.com;Port=5432;Database=<base>;Username=<usuario>;Password=<contraseña>;SSL Mode=Require'
```

.NET no carga archivos `.env` automáticamente. En Bash, carga las variables y ejecuta la API desde la raíz:

```bash
set -a
. ./.env
set +a
dotnet run --project src/GestorProyect.api/GestorProyect.api.csproj
```

En Azure, configura `ConnectionStrings__DefaultConnection` como variable de entorno en la configuración de la aplicación o usa Azure Key Vault; no despliegues el `.env` ni guardes credenciales en archivos versionados. En Azure Database for PostgreSQL, habilita SSL y permite en el firewall únicamente las IP necesarias.

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
dotnet test gestorproyect.slnx
```

Las pruebas unitarias de aplicación, dominio e infraestructura usan dependencias simuladas o EF Core InMemory. Las pruebas de integración levantan la API en memoria y requieren PostgreSQL; sin una base de pruebas exclusiva, NUnit las omite. Para ejecutarlas, configura `GESTORPROYECT_TEST_CONNECTION` con la cadena de conexión de esa base. La suite crea un esquema temporal, aplica las migraciones y lo elimina al finalizar; el usuario necesita permisos para crear y eliminar esquemas. **No uses una base de producción para estas pruebas.**
