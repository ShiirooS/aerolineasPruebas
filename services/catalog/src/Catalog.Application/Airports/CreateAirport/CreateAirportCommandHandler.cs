using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Airports;
using Catalog.Domain.Common;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Airports.CreateAirport;

public sealed class CreateAirportCommandHandler(IAirportRepository airports, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAirportCommand, CreateAirportResult>
{
    public async Task<CreateAirportResult> Handle(CreateAirportCommand request, CancellationToken cancellationToken)
    {
        var code = new AirportCode(request.Code);

        if (await airports.GetByIdAsync(code, cancellationToken) is not null)
            throw new DomainRuleViolationException($"Ya existe un aeropuerto con código {code}.");

        TimeZoneInfo timeZone;
        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(request.TimeZoneId);
        }
        catch (TimeZoneNotFoundException)
        {
            throw new ArgumentException($"'{request.TimeZoneId}' no es un identificador de zona horaria válido.", nameof(request.TimeZoneId));
        }

        var airport = new Airport(code, request.Name, request.City, request.Country, timeZone);

        await airports.AddAsync(airport, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateAirportResult(airport.Id.Value);
    }
}
