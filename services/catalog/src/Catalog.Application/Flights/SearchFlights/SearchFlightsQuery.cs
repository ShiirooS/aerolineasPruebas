using Catalog.Application.Common.ReadModels;
using Catalog.Domain.Flights;
using MediatR;

namespace Catalog.Application.Flights.SearchFlights;

public sealed record SearchFlightsQuery(
    string Origin,
    string Destination,
    DateOnly DepartureDate,
    int PassengerCount = 1,
    CabinClass? Cabin = null) : IRequest<IReadOnlyList<FlightSearchResult>>;
