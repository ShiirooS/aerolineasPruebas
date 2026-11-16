using Catalog.Application.Common.Exceptions;
using Catalog.Application.Flights.GetFlight;
using Catalog.Application.Tests.Flights.AddFareClassToFlight;
using Catalog.Application.Tests.TestDoubles;
using Catalog.Domain.Airports;
using Catalog.Domain.Flights;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Tests.Flights.GetFlight;

public class GetFlightQueryHandlerTests
{
    private static Airport CreateLim() =>
        new(new AirportCode("LIM"), "Jorge Chavez", "Lima", "Peru", TimeZoneInfo.FindSystemTimeZoneById("America/Lima"));

    private static Airport CreateScl() =>
        new(new AirportCode("SCL"), "Arturo Merino Benitez", "Santiago", "Chile", TimeZoneInfo.FindSystemTimeZoneById("America/Santiago"));

    private static Flight CreateLimSclFlight() => new(
        FlightId.New(),
        new FlightNumber(new AirlineCode("LA"), 2400),
        new AirportCode("LIM"),
        new AirportCode("SCL"),
        AircraftTypeId.New(),
        departureAt: new DateTimeOffset(2026, 10, 11, 3, 30, 0, TimeSpan.Zero),
        arrivalAt: new DateTimeOffset(2026, 10, 11, 7, 0, 0, TimeSpan.Zero));

    [Fact]
    public async Task Handle_returns_times_in_each_airport_local_offset()
    {
        var flight = CreateLimSclFlight();
        flight.AddFareClass(CabinClass.Economy, new Money(199.90m, "USD"), totalSeats: 100);
        var handler = new GetFlightQueryHandler(new InMemoryFlightRepository(flight), new InMemoryAirportRepository(CreateLim(), CreateScl()));

        var result = await handler.Handle(new GetFlightQuery(flight.Id.Value), CancellationToken.None);

        Assert.Equal("LA2400", result.FlightNumber);
        // Mismo instante, pero expresado en hora de Lima (salida) y de Santiago en verano (llegada).
        Assert.Equal(new DateTimeOffset(2026, 10, 10, 22, 30, 0, TimeSpan.FromHours(-5)), result.DepartureAt);
        Assert.Equal(TimeSpan.FromHours(-5), result.DepartureAt.Offset);
        Assert.Equal(TimeSpan.FromHours(-3), result.ArrivalAt.Offset);
        var fareClass = Assert.Single(result.FareClasses);
        Assert.NotEqual(Guid.Empty, fareClass.FareClassId);
    }

    [Fact]
    public async Task Handle_with_unknown_id_throws_not_found()
    {
        var handler = new GetFlightQueryHandler(new InMemoryFlightRepository(), new InMemoryAirportRepository());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetFlightQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
