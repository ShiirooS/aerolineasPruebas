using Catalog.Domain.Flights;
using MediatR;

namespace Catalog.Application.Flights.AddFareClassToFlight;

public sealed record AddFareClassToFlightCommand(
    Guid FlightId,
    CabinClass Cabin,
    decimal Price,
    string Currency,
    int TotalSeats) : IRequest<AddFareClassToFlightResult>;
