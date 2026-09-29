using Catalog.Application.Common.Exceptions;
using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Airlines;
using Catalog.Domain.Airports;
using Catalog.Domain.Aircraft;
using Catalog.Domain.Flights;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Flights.CreateFlight;

public sealed class CreateFlightCommandHandler(
    IFlightRepository flights,
    IAircraftTypeRepository aircraftTypes,
    IAirportRepository airports,
    IAirlineRepository airlines,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateFlightCommand, CreateFlightResult>
{
    public async Task<CreateFlightResult> Handle(CreateFlightCommand request, CancellationToken cancellationToken)
    {
        var airlineCode = new AirlineCode(request.AirlineCode);
        var origin = new AirportCode(request.Origin);
        var destination = new AirportCode(request.Destination);
        var aircraftTypeId = new AircraftTypeId(request.AircraftTypeId);

        _ = await airlines.GetByIdAsync(airlineCode, cancellationToken)
            ?? throw new NotFoundException(nameof(Airline), airlineCode.Value);
        _ = await airports.GetByIdAsync(origin, cancellationToken)
            ?? throw new NotFoundException(nameof(Airport), origin.Value);
        _ = await airports.GetByIdAsync(destination, cancellationToken)
            ?? throw new NotFoundException(nameof(Airport), destination.Value);
        _ = await aircraftTypes.GetByIdAsync(aircraftTypeId, cancellationToken)
            ?? throw new NotFoundException(nameof(AircraftType), request.AircraftTypeId);

        var flight = new Flight(
            FlightId.New(),
            new FlightNumber(airlineCode, request.FlightNumber),
            origin,
            destination,
            aircraftTypeId,
            request.DepartureAt,
            request.ArrivalAt);

        await flights.AddAsync(flight, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateFlightResult(flight.Id.Value);
    }
}
