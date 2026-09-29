using Catalog.Domain.Common;
using Catalog.Domain.Flights;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Tests;

public class FlightTests
{
    private static Flight CreateSantiagoToLimaFlight() => new(
        FlightId.New(),
        new FlightNumber(new AirlineCode("LA"), 800),
        new AirportCode("SCL"),
        new AirportCode("LIM"),
        AircraftTypeId.New(),
        departureAt: new DateTimeOffset(2026, 3, 15, 8, 0, 0, TimeSpan.FromHours(-3)),
        arrivalAt: new DateTimeOffset(2026, 3, 15, 10, 30, 0, TimeSpan.FromHours(-5)));

    [Fact]
    public void AddFareClass_reserving_within_capacity_succeeds()
    {
        var flight = CreateSantiagoToLimaFlight();
        flight.AddFareClass(CabinClass.Economy, new Money(120, "USD"), totalSeats: 3);

        var reserved = flight.TryReserveSeats(CabinClass.Economy, 2);

        Assert.True(reserved);
        Assert.Equal(1, flight.FareClasses.Single().AvailableSeats);
    }

    [Fact]
    public void TryReserveSeats_beyond_availability_fails_without_changing_state()
    {
        var flight = CreateSantiagoToLimaFlight();
        flight.AddFareClass(CabinClass.Economy, new Money(120, "USD"), totalSeats: 3);

        var reserved = flight.TryReserveSeats(CabinClass.Economy, 4);

        Assert.False(reserved);
        Assert.Equal(3, flight.FareClasses.Single().AvailableSeats);
    }

    [Fact]
    public void AddFareClass_with_duplicate_cabin_throws()
    {
        var flight = CreateSantiagoToLimaFlight();
        flight.AddFareClass(CabinClass.Economy, new Money(120, "USD"), totalSeats: 3);

        Assert.Throws<DomainRuleViolationException>(() =>
            flight.AddFareClass(CabinClass.Economy, new Money(150, "USD"), totalSeats: 5));
    }

    [Fact]
    public void Constructor_with_arrival_before_departure_throws()
    {
        Assert.Throws<ArgumentException>(() => new Flight(
            FlightId.New(),
            new FlightNumber(new AirlineCode("LA"), 800),
            new AirportCode("SCL"),
            new AirportCode("LIM"),
            AircraftTypeId.New(),
            departureAt: new DateTimeOffset(2026, 3, 15, 10, 0, 0, TimeSpan.Zero),
            arrivalAt: new DateTimeOffset(2026, 3, 15, 9, 0, 0, TimeSpan.Zero)));
    }

    [Fact]
    public void Constructor_with_same_origin_and_destination_throws()
    {
        var scl = new AirportCode("SCL");

        Assert.Throws<ArgumentException>(() => new Flight(
            FlightId.New(),
            new FlightNumber(new AirlineCode("LA"), 800),
            scl,
            scl,
            AircraftTypeId.New(),
            departureAt: DateTimeOffset.UtcNow,
            arrivalAt: DateTimeOffset.UtcNow.AddHours(1)));
    }
}
