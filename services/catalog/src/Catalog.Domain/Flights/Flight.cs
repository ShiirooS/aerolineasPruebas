using Catalog.Domain.Common;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Flights;

public sealed class Flight : Entity<FlightId>
{
    private readonly List<FareClass> _fareClasses = [];

    public FlightNumber Number { get; }
    public AirportCode Origin { get; }
    public AirportCode Destination { get; }
    public AircraftTypeId AircraftType { get; }
    public DateTimeOffset DepartureAt { get; }
    public DateTimeOffset ArrivalAt { get; }

    public IReadOnlyCollection<FareClass> FareClasses => _fareClasses.AsReadOnly();

    // Constructor privado sin parámetros para EF Core: Number es una navegación
    // owned (FlightNumber vía OwnsOne), que EF no puede bindear por constructor.
    // Mismo motivo que en FareClass.
#pragma warning disable CS8618
    private Flight()
    {
    }
#pragma warning restore CS8618

    public Flight(
        FlightId id,
        FlightNumber number,
        AirportCode origin,
        AirportCode destination,
        AircraftTypeId aircraftType,
        DateTimeOffset departureAt,
        DateTimeOffset arrivalAt)
        : base(id)
    {
        if (origin == destination)
            throw new ArgumentException("El origen y el destino de un vuelo no pueden ser el mismo aeropuerto.", nameof(destination));

        if (arrivalAt <= departureAt)
            throw new ArgumentException("La llegada debe ser posterior a la salida.", nameof(arrivalAt));

        Number = number;
        Origin = origin;
        Destination = destination;
        AircraftType = aircraftType;
        DepartureAt = departureAt;
        ArrivalAt = arrivalAt;
    }

    public FareClass AddFareClass(CabinClass cabin, Money basePrice, int totalSeats)
    {
        if (_fareClasses.Any(f => f.Cabin == cabin))
            throw new DomainRuleViolationException($"El vuelo {Number} ya tiene una tarifa para la cabina {cabin}.");

        var fareClass = new FareClass(FareClassId.New(), cabin, basePrice, totalSeats);
        _fareClasses.Add(fareClass);
        return fareClass;
    }

    public bool TryReserveSeats(CabinClass cabin, int count) =>
        GetFareClassOrThrow(cabin).TryReserveSeats(count);

    public void ReleaseSeats(CabinClass cabin, int count) =>
        GetFareClassOrThrow(cabin).ReleaseSeats(count);

    private FareClass GetFareClassOrThrow(CabinClass cabin) =>
        _fareClasses.FirstOrDefault(f => f.Cabin == cabin)
            ?? throw new DomainRuleViolationException($"El vuelo {Number} no tiene tarifa para la cabina {cabin}.");
}
