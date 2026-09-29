using MediatR;

namespace Catalog.Application.Airports.CreateAirport;

public sealed record CreateAirportCommand(
    string Code,
    string Name,
    string City,
    string Country,
    string TimeZoneId) : IRequest<CreateAirportResult>;
