# GestorProyect

Proyecto organizado en capas siguiendo los principios de Clean Architecture.

## Estructura

```text
src/
  GestorProyect.api/              # API y punto de entrada
  GestorProyect.Application/       # Servicios, interfaces, DTOs y excepciones
  GestorProyect.Domain/            # Entidades y reglas del dominio
  GestorProyect.Infratucture/      # Implementaciones técnicas e infraestructura
    Identity/
    Migrations/
    Persistence/
    Repository/
test/
  GestorProyect.Applications.Unitest/ # Pruebas unitarias de aplicación
  GestorProyect.Domain.UnitTest/      # Pruebas unitarias de dominio
  GestorProyect.Infrastructure.UnitTest/ # Pruebas unitarias de infraestructura
  GestorProyect.Api.UnitTest/          # Pruebas de OpenAPI y API
  GestorProyect.test/                 # Pruebas de integración
```

## Referencias entre capas

- **Dominio:** no depende de las demás capas.
- **Aplicación → Dominio:** los servicios de aplicación usan las entidades y conceptos del dominio.
- **Infraestructura → Aplicación y Dominio:** implementa detalles técnicos y contratos definidos por las capas internas.
- **API → Aplicación e Infraestructura:** configura la aplicación y conecta sus servicios con la infraestructura.
- **Pruebas → Aplicación:** las pruebas unitarias ejercitan los servicios y reciben el Dominio como dependencia transitiva.

## Trabajo realizado

- Se agregaron las carpetas `Identity`, `Migrations`, `Persistence` y `Repository` a Infraestructura. Cada carpeta tiene un `.gitkeep` para conservarla mientras esté vacía.
- Se agregaron pruebas unitarias de `ProyectoService` y `TareaService`, reglas de `Prioridad` y repositorios de EF Core con InMemory.
- Se agregaron las referencias entre proyectos descritas en la sección anterior.
- Se agregó `GestorProyectDbContext` para PostgreSQL y la migración inicial `InitialCreate`, con las tablas `Proyectos` y `Tareas`.
- Los servicios CRUD usan repositorios de EF Core para leer y guardar proyectos y tareas en PostgreSQL.
- Se integró Swagger UI para explorar la API y enviar solicitudes desde el navegador en `/swagger` (solo en Development). La especificación OpenAPI está en `/openapi/v1.json`; cada operación incluye resumen y descripción, y las rutas están versionadas bajo `/api/v1/proyectos` y `/api/v1/tareas`.
- Las fechas de inicio, finalización y vencimiento son fechas sin hora: la API recibe y devuelve `dd-MM-yy` (por ejemplo, `25-11-26`) y PostgreSQL las almacena como `date`.
- Se pueden consultar tareas por proyecto, estado y prioridad; consultar tareas vencidas no completadas; cambiar el estado de una tarea; y consultar el avance de un proyecto.
- La eliminación de un proyecto se rechaza mientras tenga tareas en estado `Pendiente`. Las prioridades permitidas son `Baja`, `Media` y `Alta`; los estados son `Pendiente`, `EnProgreso` y `Completada`.

## Operaciones de tareas y avance

| Operación | Ruta |
|---|---|
| Listar y filtrar tareas | `GET /api/v1/tareas?proyectoId={guid}&estado=Pendiente&prioridad=Alta` |
| Listar tareas de un proyecto | `GET /api/v1/proyectos/{id}/tareas` |
| Consultar tareas vencidas no completadas | `GET /api/v1/tareas/vencidas` |
| Cambiar el estado de una tarea | `PATCH /api/v1/tareas/{id}/estado` |
| Calcular el avance del proyecto | `GET /api/v1/proyectos/{id}/avance` |

El cambio de estado recibe un JSON como `{"estado":"Completada"}`. El avance devuelve el total de tareas, cuántas están completadas y el porcentaje completado; para un proyecto sin tareas, el porcentaje es `0`.

## Pruebas de integración

Las pruebas de integración levantan la API en memoria y usan PostgreSQL real. Configura `GESTORPROYECT_TEST_CONNECTION` con la cadena de conexión de una **base de datos exclusiva para pruebas** y ejecuta:

```bash
export GESTORPROYECT_TEST_CONNECTION='Host=<servidor>;Port=5432;Database=<base_pruebas>;Username=<usuario>;Password=<contraseña>;SSL Mode=Require'
dotnet test gestorproyect.slnx
```

La suite crea un esquema con nombre único, aplica las migraciones ahí y lo elimina al terminar; el usuario necesita permisos para crear y eliminar esquemas. Si la variable no está definida, NUnit omite las pruebas de integración y ejecuta las unitarias.

## Base de datos y migraciones

La API e Infraestructura leen la conexión PostgreSQL desde `ConnectionStrings:DefaultConnection`. Configúrala localmente mediante la variable de entorno `ConnectionStrings__DefaultConnection` y evita guardar contraseñas en archivos versionados.

Desde la raíz del repositorio, para aplicar las migraciones a la base de datos configurada:

```bash
dotnet ef database update \
  --project src/GestorProyect.Infratuctur/GestorProyect.Infratuctur.csproj \
  --startup-project src/GestorProyect.api/GestorProyect.api.csproj
```

## Ejecutar

Desde la raíz del repositorio:

```bash
dotnet build gestorproyect.slnx
dotnet test gestorproyect.slnx
```
