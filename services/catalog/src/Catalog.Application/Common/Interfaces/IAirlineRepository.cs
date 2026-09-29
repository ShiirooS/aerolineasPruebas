using Catalog.Domain.Airlines;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Common.Interfaces;

public interface IAirlineRepository
{
    Task<Airline?> GetByIdAsync(AirlineCode id, CancellationToken cancellationToken);

    Task AddAsync(Airline airline, CancellationToken cancellationToken);
}
