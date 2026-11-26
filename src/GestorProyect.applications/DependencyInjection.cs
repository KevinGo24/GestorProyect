using GestorProyect.applications.interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GestorProyect.applications;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IProyectoService, ProyectoService>();
        services.AddScoped<ITareaService, TareaService>();

        return services;
    }
}
