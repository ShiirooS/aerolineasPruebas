using MediatR;

namespace Catalog.Application.Airlines.CreateAirline;

public sealed record CreateAirlineCommand(string Code, string Name) : IRequest<CreateAirlineResult>;
