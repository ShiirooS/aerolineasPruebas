using Catalog.Application.Common.Exceptions;
using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Aircraft;
using Catalog.Domain.Flights;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Flights.AddFareClassToFlight;

public sealed class AddFareClassToFlightCommandHandler(
    IFlightRepository flights,
    IAircraftTypeRepository aircraftTypes,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddFareClassToFlightCommand, AddFareClassToFlightResult>
{
    public async Task<AddFareClassToFlightResult> Handle(AddFareClassToFlightCommand request, CancellationToken cancellationToken)
    {
        var flightId = new FlightId(request.FlightId);

        var flight = await flights.GetByIdAsync(flightId, cancellationToken)
            ?? throw new NotFoundException(nameof(Flight), request.FlightId);

        var aircraftType = await aircraftTypes.GetByIdAsync(flight.AircraftType, cancellationToken)
            ?? throw new NotFoundException(nameof(AircraftType), flight.AircraftType.Value);

        var availableCapacity = aircraftType.SeatMap.CapacityFor(request.Cabin);

        if (request.TotalSeats > availableCapacity)
            throw new SeatCapacityExceededException(request.Cabin, request.TotalSeats, availableCapacity);

        var price = new Money(request.Price, request.Currency);
        var fareClass = flight.AddFareClass(request.Cabin, price, request.TotalSeats);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AddFareClassToFlightResult(fareClass.Id.Value, fareClass.AvailableSeats);
    }
}
