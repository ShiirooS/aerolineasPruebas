using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Flights;

namespace Catalog.Application.Tests.Flights.AddFareClassToFlight;

internal sealed class InMemoryFlightRepository(params Flight[] flights) : IFlightRepository
{
    private readonly List<Flight> _flights = [.. flights];

    public Task<Flight?> GetByIdAsync(FlightId id, CancellationToken cancellationToken) =>
        Task.FromResult(_flights.FirstOrDefault(f => f.Id == id));

    public Task AddAsync(Flight flight, CancellationToken cancellationToken)
    {
        _flights.Add(flight);
        return Task.CompletedTask;
    }
}
