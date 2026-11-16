using Catalog.Domain.Aircraft;
using Catalog.Domain.Airports;
using Catalog.Domain.Flights;
using Catalog.Domain.ValueObjects;

namespace Catalog.Infrastructure.Tests;

internal static class TestData
{
    public static AircraftType CreateA320() => new(
        AircraftTypeId.New(),
        "Airbus A320",
        new SeatMap(new Dictionary<CabinClass, int> { [CabinClass.Economy] = 150, [CabinClass.Business] = 20 }));

    public static Airport CreateScl() => new(
        new AirportCode("SCL"), "Arturo Merino Benitez", "Santiago", "Chile", TimeZoneInfo.FindSystemTimeZoneById("America/Santiago"));

    public static Airport CreateLim() => new(
        new AirportCode("LIM"), "Jorge Chavez", "Lima", "Peru", TimeZoneInfo.FindSystemTimeZoneById("America/Lima"));

    // Sale 22:30 hora de Lima del 10/10/2026, que en UTC ya es 03:30 del 11/10.
    public static Flight CreateLateNightLimSclFlight(AircraftTypeId aircraftTypeId) => new(
        FlightId.New(),
        new FlightNumber(new AirlineCode("LA"), 2400),
        new AirportCode("LIM"),
        new AirportCode("SCL"),
        aircraftTypeId,
        departureAt: new DateTimeOffset(2026, 10, 10, 22, 30, 0, TimeSpan.FromHours(-5)),
        arrivalAt: new DateTimeOffset(2026, 10, 11, 4, 0, 0, TimeSpan.FromHours(-3)));

    public static Flight CreateScLimFlight(AircraftTypeId aircraftTypeId) => new(
        FlightId.New(),
        new FlightNumber(new AirlineCode("LA"), 800),
        new AirportCode("SCL"),
        new AirportCode("LIM"),
        aircraftTypeId,
        departureAt: new DateTimeOffset(2026, 3, 15, 8, 0, 0, TimeSpan.FromHours(-3)),
        arrivalAt: new DateTimeOffset(2026, 3, 15, 10, 30, 0, TimeSpan.FromHours(-5)));
}
