namespace Catalog.Application.Common.ReadModels;

public sealed record FlightSearchResult(
    Guid FlightId,
    string AirlineCode,
    string FlightNumber,
    string Origin,
    string Destination,
    DateTimeOffset DepartureAt,
    DateTimeOffset ArrivalAt,
    IReadOnlyList<FareClassOption> FareClasses);
