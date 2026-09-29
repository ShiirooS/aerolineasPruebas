namespace Catalog.Application.Flights.AddFareClassToFlight;

public sealed record AddFareClassToFlightResult(Guid FareClassId, int AvailableSeats);
