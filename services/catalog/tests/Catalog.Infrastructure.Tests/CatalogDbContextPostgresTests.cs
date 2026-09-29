using Catalog.Application.Common.ReadModels;
using Catalog.Domain.Flights;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Tests;

// Corre contra un Postgres real vía Npgsql -- el mismo motor de docker-compose
// (infra/docker-compose.yml). Existe puntualmente para confirmar si las
// comparaciones que el proveedor de SQLite no pudo traducir (igualdad de
// AirportCode, rango de DateTimeOffset - ver CatalogDbContextTests y la nota
// en FlightSearchReadModel) sí traducen bajo Npgsql, que tiene un traductor de
// consultas más completo. Apunta a localhost:5432 (la del docker-compose) por
// default; se puede apuntar a otra instancia con la variable de entorno
// AEROAPP_TEST_POSTGRES_CONNECTION. Si no hay ningún Postgres escuchando, estos
// tests fallan con un error de conexión real, no silenciosamente.
public sealed class CatalogDbContextPostgresTests : IAsyncLifetime
{
    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("AEROAPP_TEST_POSTGRES_CONNECTION")
        ?? "Host=localhost;Port=5432;Database=aeroapp;Username=aeroapp;Password=aeroapp;Search Path=catalog";

    private CatalogDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        // MigrationsHistoryTable explícito a propósito: sin esto, EF busca/crea
        // "__ef_migrations_history" en el schema por default de la conexión
        // (search_path, típicamente "public"), no en "catalog" -- aunque el
        // modelo entero use HasDefaultSchema("catalog"). La app real (ver
        // Catalog.Infrastructure/DependencyInjection.cs) fija esto mismo; sin
        // replicarlo acá, este test cree que no hay migraciones aplicadas y
        // choca contra tablas que ya existen.
        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(ConnectionString, npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "catalog"))
            .Options;

        _dbContext = new CatalogDbContext(options);
        await _dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE catalog.fare_classes, catalog.flights, catalog.aircraft_types RESTART IDENTITY CASCADE;");
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task FlightSearchReadModel_finds_matching_flight_and_excludes_cabins_without_enough_seats()
    {
        var aircraftType = TestData.CreateA320();
        var flight = TestData.CreateScLimFlight(aircraftType.Id);
        flight.AddFareClass(CabinClass.Economy, new Money(120, "USD"), totalSeats: 150);
        flight.AddFareClass(CabinClass.Business, new Money(450, "USD"), totalSeats: 2);

        _dbContext.AircraftTypes.Add(aircraftType);
        _dbContext.Flights.Add(flight);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var readModel = new FlightSearchReadModel(_dbContext);
        var results = await readModel.SearchAsync(
            new FlightSearchCriteria("SCL", "LIM", new DateOnly(2026, 3, 15), PassengerCount: 3, Cabin: null),
            CancellationToken.None);

        var result = Assert.Single(results);
        Assert.Equal("LA800", result.FlightNumber);
        // Business solo tiene 2 asientos: no alcanza para 3 pasajeros y debe quedar afuera.
        var fareClass = Assert.Single(result.FareClasses);
        Assert.Equal(CabinClass.Economy, fareClass.Cabin);
    }

    [Fact]
    public async Task FlightSearchReadModel_excludes_flights_on_a_different_date()
    {
        var aircraftType = TestData.CreateA320();
        var flight = TestData.CreateScLimFlight(aircraftType.Id);
        flight.AddFareClass(CabinClass.Economy, new Money(120, "USD"), totalSeats: 150);

        _dbContext.AircraftTypes.Add(aircraftType);
        _dbContext.Flights.Add(flight);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var readModel = new FlightSearchReadModel(_dbContext);
        var results = await readModel.SearchAsync(
            new FlightSearchCriteria("SCL", "LIM", new DateOnly(2026, 3, 16), PassengerCount: 1, Cabin: null),
            CancellationToken.None);

        Assert.Empty(results);
    }

    [Fact]
    public async Task FlightSearchReadModel_excludes_flights_to_a_different_destination()
    {
        var aircraftType = TestData.CreateA320();
        var flight = TestData.CreateScLimFlight(aircraftType.Id);
        flight.AddFareClass(CabinClass.Economy, new Money(120, "USD"), totalSeats: 150);

        _dbContext.AircraftTypes.Add(aircraftType);
        _dbContext.Flights.Add(flight);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var readModel = new FlightSearchReadModel(_dbContext);
        var results = await readModel.SearchAsync(
            new FlightSearchCriteria("SCL", "MIA", new DateOnly(2026, 3, 15), PassengerCount: 1, Cabin: null),
            CancellationToken.None);

        Assert.Empty(results);
    }
}
