using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Airlines;
using Catalog.Domain.ValueObjects;

namespace Catalog.Infrastructure.Persistence.Repositories;

internal sealed class AirlineRepository(CatalogDbContext dbContext) : IAirlineRepository
{
    public async Task<Airline?> GetByIdAsync(AirlineCode id, CancellationToken cancellationToken) =>
        await dbContext.Airlines.FindAsync([id], cancellationToken);

    public Task AddAsync(Airline airline, CancellationToken cancellationToken)
    {
        dbContext.Airlines.Add(airline);
        return Task.CompletedTask;
    }
}
