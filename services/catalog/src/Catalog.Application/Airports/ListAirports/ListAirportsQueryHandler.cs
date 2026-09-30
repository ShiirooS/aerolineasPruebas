using Catalog.Application.Common.Interfaces;
using MediatR;

namespace Catalog.Application.Airports.ListAirports;

public sealed class ListAirportsQueryHandler(IAirportRepository airports)
    : IRequestHandler<ListAirportsQuery, IReadOnlyList<AirportResult>>
{
    public async Task<IReadOnlyList<AirportResult>> Handle(ListAirportsQuery request, CancellationToken cancellationToken)
    {
        var all = await airports.ListAsync(cancellationToken);

        return all
            .Select(AirportResult.From)
            .OrderBy(a => a.Code, StringComparer.Ordinal)
            .ToList();
    }
}
