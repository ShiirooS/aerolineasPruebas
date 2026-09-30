using Catalog.Application.Common.Exceptions;
using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Airports;
using Catalog.Domain.Flights;
using MediatR;

namespace Catalog.Application.Flights.GetFlight;

public sealed class GetFlightQueryHandler(IFlightRepository flights, IAirportRepository airports)
    : IRequestHandler<GetFlightQuery, FlightDetailsResult>
{
    public async Task<FlightDetailsResult> Handle(GetFlightQuery request, CancellationToken cancellationToken)
    {
        var flight = await flights.GetByIdAsync(new FlightId(request.FlightId), cancellationToken)
            ?? throw new NotFoundException(nameof(Flight), request.FlightId);

        var origin = await airports.GetByIdAsync(flight.Origin, cancellationToken)
            ?? throw new NotFoundException(nameof(Airport), flight.Origin.Value);
        var destination = await airports.GetByIdAsync(flight.Destination, cancellationToken)
            ?? throw new NotFoundException(nameof(Airport), flight.Destination.Value);

        return new FlightDetailsResult(
            flight.Id.Value,
            flight.Number.Airline.Value,
            flight.Number.ToString(),
            flight.Origin.Value,
            flight.Destination.Value,
            flight.AircraftType.Value,
            origin.ToLocalTime(flight.DepartureAt),
            destination.ToLocalTime(flight.ArrivalAt),
            flight.FareClasses
                .OrderBy(fc => fc.Cabin)
                .Select(fc => new FareClassDetails(fc.Id.Value, fc.Cabin, fc.BasePrice.Amount, fc.BasePrice.Currency, fc.AvailableSeats))
                .ToList());
    }
}
