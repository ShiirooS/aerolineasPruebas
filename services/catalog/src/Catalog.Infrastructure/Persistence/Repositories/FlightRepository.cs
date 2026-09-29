using Catalog.Application.Common.Interfaces;
using Catalog.Domain.Flights;

namespace Catalog.Infrastructure.Persistence.Repositories;

internal sealed class FlightRepository(CatalogDbContext dbContext) : IFlightRepository
{
    // FindAsync (no un Where(f => f.Id == id)) a propósito: es la forma soportada
    // de buscar por una clave con HasConversion sin depender de que el traductor
    // de LINQ-a-SQL sepa "ver a través" del operador == generado para un record
    // struct como FlightId. De paso, resuelve primero contra entidades ya
    // trackeadas en este DbContext antes de ir a la base.
    public async Task<Flight?> GetByIdAsync(FlightId id, CancellationToken cancellationToken) =>
        await dbContext.Flights.FindAsync([id], cancellationToken);

    public Task AddAsync(Flight flight, CancellationToken cancellationToken)
    {
        dbContext.Flights.Add(flight);
        return Task.CompletedTask;
    }
}
