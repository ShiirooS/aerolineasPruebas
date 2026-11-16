using Catalog.Domain.Airports;

namespace Catalog.Application.Airports;

public sealed record AirportResult(string Code, string Name, string City, string Country, string TimeZoneId)
{
    public static AirportResult From(Airport airport) =>
        new(airport.Id.Value, airport.Name, airport.City, airport.Country, airport.TimeZone.Id);
}
