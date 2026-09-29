using MediatR;

namespace Catalog.Application.Flights.CreateFlight;

public sealed record CreateFlightCommand(
    string AirlineCode,
    int FlightNumber,
    string Origin,
    string Destination,
    Guid AircraftTypeId,
    DateTimeOffset DepartureAt,
    DateTimeOffset ArrivalAt) : IRequest<CreateFlightResult>;
