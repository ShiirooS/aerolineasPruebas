using MediatR;

namespace Catalog.Application.Flights.GetFlight;

public sealed record GetFlightQuery(Guid FlightId) : IRequest<FlightDetailsResult>;
