using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Airports;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Repositories;

internal sealed class AirportRepository(CatalogDbContext dbContext) : IAirportRepository
{
    public async Task<Airport?> GetByIdAsync(AirportCode id, CancellationToken cancellationToken) =>
        await dbContext.Airports.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Airport>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.Airports.AsNoTracking().ToListAsync(cancellationToken);

    public Task AddAsync(Airport airport, CancellationToken cancellationToken)
    {
        dbContext.Airports.Add(airport);
        return Task.CompletedTask;
    }
}
