using Catalog.Domain.Flights;
using MediatR;

namespace Catalog.Application.Aircraft.CreateAircraftType;

public sealed record CreateAircraftTypeCommand(
    string Model,
    IReadOnlyDictionary<CabinClass, int> SeatMap) : IRequest<CreateAircraftTypeResult>;
