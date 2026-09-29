using Catalog.Application.Common.Exceptions;
using Catalog.Application.Flights.CreateFlight;
using Catalog.Application.Tests.Flights.AddFareClassToFlight;
using Catalog.Application.Tests.TestDoubles;
using Catalog.Domain.Aircraft;
using Catalog.Domain.Airlines;
using Catalog.Domain.Airports;
using Catalog.Domain.Flights;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Tests.Flights.CreateFlight;

public class CreateFlightCommandHandlerTests
{
    private static Airline CreateLatam() => new(new AirlineCode("LA"), "LATAM Airlines");
    private static Airport CreateScl() => new(new AirportCode("SCL"), "Arturo Merino Benitez", "Santiago", "Chile", TimeZoneInfo.Utc);
    private static Airport CreateLim() => new(new AirportCode("LIM"), "Jorge Chavez", "Lima", "Peru", TimeZoneInfo.Utc);
    private static AircraftType CreateA320() => new(
        AircraftTypeId.New(), "Airbus A320", new SeatMap(new Dictionary<CabinClass, int> { [CabinClass.Economy] = 150 }));

    [Fact]
    public async Task Handle_with_valid_references_creates_flight()
    {
        var aircraftType = CreateA320();
        var handler = new CreateFlightCommandHandler(
            new InMemoryFlightRepository(),
            new InMemoryAircraftTypeRepository(aircraftType),
            new InMemoryAirportRepository(CreateScl(), CreateLim()),
            new InMemoryAirlineRepository(CreateLatam()),
            new NoOpUnitOfWork());

        var result = await handler.Handle(
            new CreateFlightCommand("LA", 800, "SCL", "LIM", aircraftType.Id.Value,
                new DateTimeOffset(2026, 3, 15, 8, 0, 0, TimeSpan.FromHours(-3)),
                new DateTimeOffset(2026, 3, 15, 10, 30, 0, TimeSpan.FromHours(-5))),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.FlightId);
    }

    [Fact]
    public async Task Handle_with_unknown_origin_throws_not_found()
    {
        var aircraftType = CreateA320();
        var handler = new CreateFlightCommandHandler(
            new InMemoryFlightRepository(),
            new InMemoryAircraftTypeRepository(aircraftType),
            new InMemoryAirportRepository(CreateLim()), // sin SCL a propósito
            new InMemoryAirlineRepository(CreateLatam()),
            new NoOpUnitOfWork());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(
            new CreateFlightCommand("LA", 800, "SCL", "LIM", aircraftType.Id.Value,
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2)),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_with_unknown_airline_throws_not_found()
    {
        var aircraftType = CreateA320();
        var flights = new InMemoryFlightRepository();
        var handler = new CreateFlightCommandHandler(
            flights,
            new InMemoryAircraftTypeRepository(aircraftType),
            new InMemoryAirportRepository(CreateScl(), CreateLim()),
            new InMemoryAirlineRepository(),
            new NoOpUnitOfWork());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(
            new CreateFlightCommand("LA", 800, "SCL", "LIM", aircraftType.Id.Value,
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2)),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_with_unknown_aircraft_type_throws_not_found()
    {
        var handler = new CreateFlightCommandHandler(
            new InMemoryFlightRepository(),
            new InMemoryAircraftTypeRepository(),
            new InMemoryAirportRepository(CreateScl(), CreateLim()),
            new InMemoryAirlineRepository(CreateLatam()),
            new NoOpUnitOfWork());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(
            new CreateFlightCommand("LA", 800, "SCL", "LIM", Guid.NewGuid(),
                DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2)),
            CancellationToken.None));
    }
}
