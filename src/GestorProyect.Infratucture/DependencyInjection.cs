using GestorProyect.applications.interfaces;
using GestorProyect.Infratuctur.Persistence;
using GestorProyect.Infratuctur.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestorProyect.Infratuctur;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se configuró la cadena DefaultConnection.");

        services.AddDbContext<GestorProyectDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IProyectoRepository, ProyectoRepository>();
        services.AddScoped<ITareaRepository, TareaRepository>();

        return services;
    }
}
