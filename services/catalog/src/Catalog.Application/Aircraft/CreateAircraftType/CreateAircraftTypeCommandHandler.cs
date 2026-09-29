using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Aircraft;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Aircraft.CreateAircraftType;

public sealed class CreateAircraftTypeCommandHandler(IAircraftTypeRepository aircraftTypes, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateAircraftTypeCommand, CreateAircraftTypeResult>
{
    public async Task<CreateAircraftTypeResult> Handle(CreateAircraftTypeCommand request, CancellationToken cancellationToken)
    {
        var seatMap = new SeatMap(request.SeatMap);
        var aircraftType = new AircraftType(AircraftTypeId.New(), request.Model, seatMap);

        await aircraftTypes.AddAsync(aircraftType, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateAircraftTypeResult(aircraftType.Id.Value);
    }
}
