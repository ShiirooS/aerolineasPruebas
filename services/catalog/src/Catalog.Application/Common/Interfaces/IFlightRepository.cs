using Catalog.Domain.Flights;

namespace Catalog.Application.Common.Interfaces;

public interface IFlightRepository
{
    Task<Flight?> GetByIdAsync(FlightId id, CancellationToken cancellationToken);

    Task AddAsync(Flight flight, CancellationToken cancellationToken);
}
