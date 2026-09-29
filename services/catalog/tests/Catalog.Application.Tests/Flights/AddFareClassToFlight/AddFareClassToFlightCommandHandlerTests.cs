using Catalog.Application.Common.Exceptions;
using Catalog.Application.Flights.AddFareClassToFlight;
using Catalog.Application.Tests.TestDoubles;
using Catalog.Domain.Aircraft;
using Catalog.Domain.Common;
using Catalog.Domain.Flights;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Tests.Flights.AddFareClassToFlight;

public class AddFareClassToFlightCommandHandlerTests
{
    private static Flight CreateFlight(AircraftTypeId aircraftTypeId) => new(
        FlightId.New(),
        new FlightNumber(new AirlineCode("LA"), 800),
        new AirportCode("SCL"),
        new AirportCode("LIM"),
        aircraftTypeId,
        departureAt: new DateTimeOffset(2026, 3, 15, 8, 0, 0, TimeSpan.FromHours(-3)),
        arrivalAt: new DateTimeOffset(2026, 3, 15, 10, 30, 0, TimeSpan.FromHours(-5)));

    private static AircraftType CreateAircraftType(AircraftTypeId id, int economyCapacity = 150) => new(
        id,
        "Airbus A320",
        new SeatMap(new Dictionary<CabinClass, int> { [CabinClass.Economy] = economyCapacity }));

    [Fact]
    public async Task Handle_within_capacity_adds_fare_class_and_persists()
    {
        var aircraftType = CreateAircraftType(AircraftTypeId.New());
        var flight = CreateFlight(aircraftType.Id);
        var unitOfWork = new NoOpUnitOfWork();

        var handler = new AddFareClassToFlightCommandHandler(
            new InMemoryFlightRepository(flight),
            new InMemoryAircraftTypeRepository(aircraftType),
            unitOfWork);

        var result = await handler.Handle(
            new AddFareClassToFlightCommand(flight.Id.Value, CabinClass.Economy, Price: 120, Currency: "USD", TotalSeats: 150),
            CancellationToken.None);

        Assert.Equal(150, result.AvailableSeats);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_exceeding_aircraft_capacity_throws_and_does_not_persist()
    {
        var aircraftType = CreateAircraftType(AircraftTypeId.New(), economyCapacity: 150);
        var flight = CreateFlight(aircraftType.Id);
        var unitOfWork = new NoOpUnitOfWork();

        var handler = new AddFareClassToFlightCommandHandler(
            new InMemoryFlightRepository(flight),
            new InMemoryAircraftTypeRepository(aircraftType),
            unitOfWork);

        await Assert.ThrowsAsync<SeatCapacityExceededException>(() => handler.Handle(
            new AddFareClassToFlightCommand(flight.Id.Value, CabinClass.Economy, Price: 120, Currency: "USD", TotalSeats: 151),
            CancellationToken.None));

        Assert.Empty(flight.FareClasses);
        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_with_unknown_flight_throws_not_found()
    {
        var aircraftType = CreateAircraftType(AircraftTypeId.New());
        var handler = new AddFareClassToFlightCommandHandler(
            new InMemoryFlightRepository(),
            new InMemoryAircraftTypeRepository(aircraftType),
            new NoOpUnitOfWork());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(
            new AddFareClassToFlightCommand(Guid.NewGuid(), CabinClass.Economy, Price: 120, Currency: "USD", TotalSeats: 10),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_with_unknown_aircraft_type_throws_not_found()
    {
        var flight = CreateFlight(AircraftTypeId.New());
        var handler = new AddFareClassToFlightCommandHandler(
            new InMemoryFlightRepository(flight),
            new InMemoryAircraftTypeRepository(),
            new NoOpUnitOfWork());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(
            new AddFareClassToFlightCommand(flight.Id.Value, CabinClass.Economy, Price: 120, Currency: "USD", TotalSeats: 10),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_with_duplicate_cabin_propagates_domain_rule_violation()
    {
        var aircraftType = CreateAircraftType(AircraftTypeId.New());
        var flight = CreateFlight(aircraftType.Id);
        flight.AddFareClass(CabinClass.Economy, new Money(100, "USD"), totalSeats: 100);
        var unitOfWork = new NoOpUnitOfWork();

        var handler = new AddFareClassToFlightCommandHandler(
            new InMemoryFlightRepository(flight),
            new InMemoryAircraftTypeRepository(aircraftType),
            unitOfWork);

        await Assert.ThrowsAsync<DomainRuleViolationException>(() => handler.Handle(
            new AddFareClassToFlightCommand(flight.Id.Value, CabinClass.Economy, Price: 120, Currency: "USD", TotalSeats: 50),
            CancellationToken.None));

        Assert.Equal(0, unitOfWork.SaveChangesCallCount);
    }
}
