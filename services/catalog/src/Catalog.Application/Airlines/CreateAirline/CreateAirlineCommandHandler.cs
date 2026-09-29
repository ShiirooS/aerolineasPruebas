using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Airlines;
using Catalog.Domain.Common;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Airlines.CreateAirline;

public sealed class CreateAirlineCommandHandler(IAirlineRepository airlines, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAirlineCommand, CreateAirlineResult>
{
    public async Task<CreateAirlineResult> Handle(CreateAirlineCommand request, CancellationToken cancellationToken)
    {
        var code = new AirlineCode(request.Code);

        if (await airlines.GetByIdAsync(code, cancellationToken) is not null)
            throw new DomainRuleViolationException($"Ya existe una aerolínea con código {code}.");

        var airline = new Airline(code, request.Name);

        await airlines.AddAsync(airline, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateAirlineResult(airline.Id.Value);
    }
}
