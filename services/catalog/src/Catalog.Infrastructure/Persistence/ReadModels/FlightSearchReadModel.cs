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
        var dayStart = new DateTimeOffset(criteria.DepartureDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1);

        // NOTA sobre verificación: este Where no se pudo confirmar contra SQLite.
        // Un test de integración (ver CatalogDbContextTests) mostró que ni la
        // igualdad sobre AirportCode ni la comparación de rango sobre DateTimeOffset
        // traducen bajo el proveedor de SQLite — pero ambas son limitaciones
        // documentadas de ESE proveedor específico (SQLite no tiene tipo nativo de
        // fecha con offset ni sabe traducir el == generado de un record de tipo
        // referencia), no necesariamente de Npgsql/Postgres, que sí soporta
        // timestamptz nativo y tiene un traductor de consultas más completo. Queda
        // pendiente confirmar esto en vivo contra Postgres real (falta Docker); si
        // falla ahí también, la salida es SQL crudo (FromSqlInterpolated) o filtrar
        // en memoria como se probó en un intento anterior de este mismo archivo.
        var flights = await dbContext.Flights
            .AsNoTracking()
            .Where(f => f.Origin == origin && f.Destination == destination)
            .Where(f => f.DepartureAt >= dayStart && f.DepartureAt < dayEnd)
            .ToListAsync(cancellationToken);

        return flights
            .Select(f => new FlightSearchResult(
                f.Id.Value,
                f.Number.Airline.Value,
                f.Number.ToString(),
                f.Origin.Value,
                f.Destination.Value,
                f.DepartureAt,
                f.ArrivalAt,
                f.FareClasses
                    .Where(fc => fc.AvailableSeats >= criteria.PassengerCount)
                    .Where(fc => criteria.Cabin == null || fc.Cabin == criteria.Cabin)
                    .Select(fc => new FareClassOption(fc.Cabin, fc.BasePrice.Amount, fc.BasePrice.Currency, fc.AvailableSeats))
                    .ToList()))
            .Where(result => result.FareClasses.Count > 0)
            .ToList();
    }
}
