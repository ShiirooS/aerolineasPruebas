using Catalog.Application.Common.Exceptions;
using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Airlines;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Airlines.GetAirline;

public sealed class GetAirlineQueryHandler(IAirlineRepository airlines)
    : IRequestHandler<GetAirlineQuery, AirlineResult>
{
    public async Task<AirlineResult> Handle(GetAirlineQuery request, CancellationToken cancellationToken)
    {
        var code = new AirlineCode(request.Code);

        var airline = await airlines.GetByIdAsync(code, cancellationToken)
            ?? throw new NotFoundException(nameof(Airline), code.Value);

        return new AirlineResult(airline.Id.Value, airline.Name);
    }
}
