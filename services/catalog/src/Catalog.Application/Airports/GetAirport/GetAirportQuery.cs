using MediatR;

namespace Catalog.Application.Airports.GetAirport;

public sealed record GetAirportQuery(string Code) : IRequest<AirportResult>;
