using Catalog.Application.Common.Exceptions;
using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Aircraft;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Aircraft.GetAircraftType;

public sealed class GetAircraftTypeQueryHandler(IAircraftTypeRepository aircraftTypes)
    : IRequestHandler<GetAircraftTypeQuery, AircraftTypeResult>
{
    public async Task<AircraftTypeResult> Handle(GetAircraftTypeQuery request, CancellationToken cancellationToken)
    {
        var aircraftType = await aircraftTypes.GetByIdAsync(new AircraftTypeId(request.AircraftTypeId), cancellationToken)
            ?? throw new NotFoundException(nameof(AircraftType), request.AircraftTypeId);

        return new AircraftTypeResult(
            aircraftType.Id.Value,
            aircraftType.Model,
            aircraftType.SeatMap.ToDictionary(entry => entry.Key, entry => entry.Value));
    }
}
