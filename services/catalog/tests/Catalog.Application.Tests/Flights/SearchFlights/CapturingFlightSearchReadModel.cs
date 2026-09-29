using Catalog.Application.Common.ReadModels;

namespace Catalog.Application.Tests.Flights.SearchFlights;

internal sealed class CapturingFlightSearchReadModel(params FlightSearchResult[] results) : IFlightSearchReadModel
{
    public FlightSearchCriteria? LastCriteria { get; private set; }

    public Task<IReadOnlyList<FlightSearchResult>> SearchAsync(FlightSearchCriteria criteria, CancellationToken cancellationToken)
    {
        LastCriteria = criteria;
        return Task.FromResult<IReadOnlyList<FlightSearchResult>>(results);
    }
}
