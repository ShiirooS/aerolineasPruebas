using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Aircraft;
using Catalog.Domain.ValueObjects;

namespace Catalog.Infrastructure.Persistence.Repositories;

internal sealed class AircraftTypeRepository(CatalogDbContext dbContext) : IAircraftTypeRepository
{
    public async Task<AircraftType?> GetByIdAsync(AircraftTypeId id, CancellationToken cancellationToken) =>
        await dbContext.AircraftTypes.FindAsync([id], cancellationToken);

    public Task AddAsync(AircraftType aircraftType, CancellationToken cancellationToken)
    {
        dbContext.AircraftTypes.Add(aircraftType);
        return Task.CompletedTask;
    }
}
