using MediatR;

namespace Catalog.Application.Aircraft.GetAircraftType;

public sealed record GetAircraftTypeQuery(Guid AircraftTypeId) : IRequest<AircraftTypeResult>;
