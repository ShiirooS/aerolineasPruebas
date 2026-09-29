using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Aircraft;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Tests.Flights.AddFareClassToFlight;

internal sealed class InMemoryAircraftTypeRepository(params AircraftType[] aircraftTypes) : IAircraftTypeRepository
{
    private readonly List<AircraftType> _aircraftTypes = [.. aircraftTypes];

    public Task<AircraftType?> GetByIdAsync(AircraftTypeId id, CancellationToken cancellationToken) =>
        Task.FromResult(_aircraftTypes.FirstOrDefault(a => a.Id == id));

    public Task AddAsync(AircraftType aircraftType, CancellationToken cancellationToken)
    {
        _aircraftTypes.Add(aircraftType);
        return Task.CompletedTask;
    }
}
