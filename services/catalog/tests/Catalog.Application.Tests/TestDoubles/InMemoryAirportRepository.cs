using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Airports;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Tests.TestDoubles;

internal sealed class InMemoryAirportRepository(params Airport[] airports) : IAirportRepository
{
    private readonly List<Airport> _airports = [.. airports];

    public Task<Airport?> GetByIdAsync(AirportCode id, CancellationToken cancellationToken) =>
        Task.FromResult(_airports.FirstOrDefault(a => a.Id == id));

    public Task AddAsync(Airport airport, CancellationToken cancellationToken)
    {
        _airports.Add(airport);
        return Task.CompletedTask;
    }
}
