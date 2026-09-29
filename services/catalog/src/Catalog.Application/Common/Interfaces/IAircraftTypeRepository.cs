using Catalog.Domain.Aircraft;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Common.Interfaces;

public interface IAircraftTypeRepository
{
    Task<AircraftType?> GetByIdAsync(AircraftTypeId id, CancellationToken cancellationToken);

    Task AddAsync(AircraftType aircraftType, CancellationToken cancellationToken);
}
