using Catalog.Application.Common.ReadModels;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Flights.SearchFlights;

public sealed class SearchFlightsQueryHandler(IFlightSearchReadModel readModel)
    : IRequestHandler<SearchFlightsQuery, IReadOnlyList<FlightSearchResult>>
{
    public Task<IReadOnlyList<FlightSearchResult>> Handle(SearchFlightsQuery request, CancellationToken cancellationToken)
    {
        if (request.PassengerCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(request.PassengerCount), request.PassengerCount, "La cantidad de pasajeros debe ser mayor a cero.");

        var origin = new AirportCode(request.Origin);
        var destination = new AirportCode(request.Destination);

        if (origin == destination)
            throw new ArgumentException("El origen y el destino no pueden ser el mismo aeropuerto.", nameof(request.Destination));

        var criteria = new FlightSearchCriteria(origin.Value, destination.Value, request.DepartureDate, request.PassengerCount, request.Cabin);

        return readModel.SearchAsync(criteria, cancellationToken);
    }
}
