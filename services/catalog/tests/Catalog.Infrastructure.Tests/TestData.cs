using Catalog.Domain.Aircraft;
using Catalog.Domain.Flights;
using Catalog.Domain.ValueObjects;

namespace Catalog.Infrastructure.Tests;

internal static class TestData
{
    public static AircraftType CreateA320() => new(
        AircraftTypeId.New(),
        "Airbus A320",
        new SeatMap(new Dictionary<CabinClass, int> { [CabinClass.Economy] = 150, [CabinClass.Business] = 20 }));

    public static Flight CreateScLimFlight(AircraftTypeId aircraftTypeId) => new(
        FlightId.New(),
        new FlightNumber(new AirlineCode("LA"), 800),
        new AirportCode("SCL"),
        new AirportCode("LIM"),
        aircraftTypeId,
        departureAt: new DateTimeOffset(2026, 3, 15, 8, 0, 0, TimeSpan.FromHours(-3)),
        arrivalAt: new DateTimeOffset(2026, 3, 15, 10, 30, 0, TimeSpan.FromHours(-5)));
}
