using Catalog.Domain.Airports;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Common.Interfaces;

public interface IAirportRepository
{
    Task<Airport?> GetByIdAsync(AirportCode id, CancellationToken cancellationToken);

    Task AddAsync(Airport airport, CancellationToken cancellationToken);
}
