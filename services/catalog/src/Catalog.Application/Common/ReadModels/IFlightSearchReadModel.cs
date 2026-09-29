namespace Catalog.Application.Common.ReadModels;

public interface IFlightSearchReadModel
{
    Task<IReadOnlyList<FlightSearchResult>> SearchAsync(FlightSearchCriteria criteria, CancellationToken cancellationToken);
}
