using Catalog.Application.Common.Interfaces;
using Catalog.Application.Common.ReadModels;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.ReadModels;
using Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CatalogDb")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'CatalogDb'.");

        services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "catalog")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<CatalogDbContext>());
        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<IAircraftTypeRepository, AircraftTypeRepository>();
        services.AddScoped<IAirportRepository, AirportRepository>();
        services.AddScoped<IAirlineRepository, AirlineRepository>();
        services.AddScoped<IFlightSearchReadModel, FlightSearchReadModel>();

        return services;
    }
}
