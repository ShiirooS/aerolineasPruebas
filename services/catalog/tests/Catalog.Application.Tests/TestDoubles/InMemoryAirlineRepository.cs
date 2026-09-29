using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Airlines;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Tests.TestDoubles;

internal sealed class InMemoryAirlineRepository(params Airline[] airlines) : IAirlineRepository
{
    private readonly List<Airline> _airlines = [.. airlines];

    public Task<Airline?> GetByIdAsync(AirlineCode id, CancellationToken cancellationToken) =>
        Task.FromResult(_airlines.FirstOrDefault(a => a.Id == id));

    public Task AddAsync(Airline airline, CancellationToken cancellationToken)
    {
        _airlines.Add(airline);
        return Task.CompletedTask;
    }
}
