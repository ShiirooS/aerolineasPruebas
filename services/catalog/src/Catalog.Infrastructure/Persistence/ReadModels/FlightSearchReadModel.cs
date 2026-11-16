using Catalog.Application.Common.ReadModels;
using Catalog.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.ReadModels;

internal sealed class FlightSearchReadModel(CatalogDbContext dbContext) : IFlightSearchReadModel
{
    public async Task<IReadOnlyList<FlightSearchResult>> SearchAsync(FlightSearchCriteria criteria, CancellationToken cancellationToken)
    {
        var origin = new AirportCode(criteria.Origin);
        var destination = new AirportCode(criteria.Destination);

        // Los aeropuertos se cargan primero porque la fecha buscada es el día LOCAL
        // del origen (un vuelo que sale 22:30 en Lima el 10/10 es "del 10/10",
        // aunque en UTC ya sea el 11/10). Si alguno no existe, no puede haber vuelos.
        var originAirport = await dbContext.Airports.FindAsync([origin], cancellationToken);
        var destinationAirport = await dbContext.Airports.FindAsync([destination], cancellationToken);

        if (originAirport is null || destinationAirport is null)
            return [];

        var (dayStartUtc, dayEndUtc) = originAirport.LocalDayBoundsUtc(criteria.DepartureDate);

        var flights = await dbContext.Flights
            .AsNoTracking()
            .Where(f => f.Origin == origin && f.Destination == destination)
            .Where(f => f.DepartureAt >= dayStartUtc && f.DepartureAt < dayEndUtc)
            .OrderBy(f => f.DepartureAt)
            .ToListAsync(cancellationToken);

        return flights
            .Select(f => new FlightSearchResult(
                f.Id.Value,
                f.Number.Airline.Value,
                f.Number.ToString(),
                f.Origin.Value,
                f.Destination.Value,
                // Cada hora con el offset de SU aeropuerto: salida en hora del origen,
                // llegada en hora del destino, como en una tarjeta de embarque.
                originAirport.ToLocalTime(f.DepartureAt),
                destinationAirport.ToLocalTime(f.ArrivalAt),
                f.FareClasses
                    .Where(fc => fc.AvailableSeats >= criteria.PassengerCount)
                    .Where(fc => criteria.Cabin == null || fc.Cabin == criteria.Cabin)
                    .Select(fc => new FareClassOption(fc.Cabin, fc.BasePrice.Amount, fc.BasePrice.Currency, fc.AvailableSeats))
                    .ToList()))
            .Where(result => result.FareClasses.Count > 0)
            .ToList();
    }
}
