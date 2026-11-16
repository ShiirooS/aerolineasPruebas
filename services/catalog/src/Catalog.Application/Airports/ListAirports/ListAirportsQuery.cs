using MediatR;

namespace Catalog.Application.Airports.ListAirports;

public sealed record ListAirportsQuery : IRequest<IReadOnlyList<AirportResult>>;
