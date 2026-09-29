using Catalog.Domain.Flights;

namespace Catalog.Application.Common.ReadModels;

public sealed record FlightSearchCriteria(
    string Origin,
    string Destination,
    DateOnly DepartureDate,
    int PassengerCount,
    CabinClass? Cabin);
