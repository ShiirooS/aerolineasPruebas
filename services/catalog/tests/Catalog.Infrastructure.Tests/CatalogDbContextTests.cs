using Catalog.Domain.Flights;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Persistence;
using Catalog.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Tests;

// SQLite en memoria, no Postgres real: rápido y sin dependencias externas, pero
// su traductor de LINQ-a-SQL es más limitado que el de Npgsql (ver
// CatalogDbContextPostgresTests para las queries que SQLite no logra traducir).
// Lo que SÍ valida acá, sin necesitar nada externo: que el modelo EF Core
// materializa bien - constructores privados, navegaciones owned detrás de
// campos privados, conversión de SeatMap a/desde JSON.
public sealed class CatalogDbContextTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly CatalogDbContext _dbContext;

    public CatalogDbContextTests()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new CatalogDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task AircraftType_seat_map_round_trips_through_json_conversion()
    {
        var aircraftType = TestData.CreateA320();
        _dbContext.AircraftTypes.Add(aircraftType);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var reloaded = await new AircraftTypeRepository(_dbContext).GetByIdAsync(aircraftType.Id, CancellationToken.None);

        Assert.NotNull(reloaded);
        Assert.Equal(150, reloaded!.SeatMap.CapacityFor(CabinClass.Economy));
        Assert.Equal(20, reloaded.SeatMap.CapacityFor(CabinClass.Business));
        Assert.Equal(0, reloaded.SeatMap.CapacityFor(CabinClass.First));
    }

    [Fact]
    public async Task FlightRepository_GetById_rehydrates_owned_fare_classes()
    {
        var aircraftType = TestData.CreateA320();
        var flight = TestData.CreateScLimFlight(aircraftType.Id);
        flight.AddFareClass(CabinClass.Economy, new Money(120, "USD"), totalSeats: 150);
        flight.AddFareClass(CabinClass.Business, new Money(450, "USD"), totalSeats: 20);

        _dbContext.AircraftTypes.Add(aircraftType);
        _dbContext.Flights.Add(flight);
        await _dbContext.SaveChangesAsync();
        _dbContext.ChangeTracker.Clear();

        var reloaded = await new FlightRepository(_dbContext).GetByIdAsync(flight.Id, CancellationToken.None);

        Assert.NotNull(reloaded);
        Assert.Equal(2, reloaded!.FareClasses.Count);
        Assert.True(reloaded.TryReserveSeats(CabinClass.Economy, 5));
    }
}
