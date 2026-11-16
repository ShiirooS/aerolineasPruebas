using Catalog.Application.Common.Exceptions;
using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Airports;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Airports.GetAirport;

public sealed class GetAirportQueryHandler(IAirportRepository airports)
    : IRequestHandler<GetAirportQuery, AirportResult>
{
    public async Task<AirportResult> Handle(GetAirportQuery request, CancellationToken cancellationToken)
    {
        var code = new AirportCode(request.Code);

        var airport = await airports.GetByIdAsync(code, cancellationToken)
            ?? throw new NotFoundException(nameof(Airport), code.Value);

        return AirportResult.From(airport);
    }
}
