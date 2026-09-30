using MediatR;

namespace Catalog.Application.Airlines.GetAirline;

public sealed record GetAirlineQuery(string Code) : IRequest<AirlineResult>;
