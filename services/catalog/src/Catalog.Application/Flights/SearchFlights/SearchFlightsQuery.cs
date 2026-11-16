using Catalog.Application.Common.ReadModels;
using Catalog.Domain.Flights;
using MediatR;

namespace Catalog.Application.Flights.SearchFlights;

// DepartureDate es nullable a propósito: con DateOnly a secas, un ?departureDate
// ausente en la URL se bindea en silencio como 0001-01-01 y la búsqueda devuelve
// [] (o revienta al calcular el día local) en vez de avisar que falta el dato.
public sealed record SearchFlightsQuery(
    string Origin,
    string Destination,
    DateOnly? DepartureDate,
    int PassengerCount = 1,
    CabinClass? Cabin = null) : IRequest<IReadOnlyList<FlightSearchResult>>;
