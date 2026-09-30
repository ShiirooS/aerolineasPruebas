using Catalog.Domain.Flights;

namespace Catalog.Application.Flights.GetFlight;

public sealed record FlightDetailsResult(
    Guid FlightId,
    string AirlineCode,
    string FlightNumber,
    string Origin,
    string Destination,
    Guid AircraftTypeId,
    DateTimeOffset DepartureAt,
    DateTimeOffset ArrivalAt,
    IReadOnlyList<FareClassDetails> FareClasses);

// A diferencia de FareClassOption (búsqueda), expone el id: Booking lo va a
// necesitar para reservar una tarifa concreta.
public sealed record FareClassDetails(
    Guid FareClassId,
    CabinClass Cabin,
    decimal Price,
    string Currency,
    int AvailableSeats);
