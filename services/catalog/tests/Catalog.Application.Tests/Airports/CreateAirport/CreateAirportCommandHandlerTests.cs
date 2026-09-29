using Catalog.Application.Airports.CreateAirport;
using Catalog.Application.Tests.TestDoubles;
using Catalog.Domain.Airports;
using Catalog.Domain.Common;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Tests.Airports.CreateAirport;

public class CreateAirportCommandHandlerTests
{
    [Fact]
    public async Task Handle_with_new_code_creates_airport()
    {
        var repository = new InMemoryAirportRepository();
        var handler = new CreateAirportCommandHandler(repository, new NoOpUnitOfWork());

        var result = await handler.Handle(
            new CreateAirportCommand("scl", "Arturo Merino Benitez", "Santiago", "Chile", "America/Santiago"),
            CancellationToken.None);

        Assert.Equal("SCL", result.Code);
    }

    [Fact]
    public async Task Handle_with_existing_code_throws()
    {
        var existing = new Airport(new AirportCode("SCL"), "Existente", "Santiago", "Chile", TimeZoneInfo.Utc);
        var repository = new InMemoryAirportRepository(existing);
        var handler = new CreateAirportCommandHandler(repository, new NoOpUnitOfWork());

        await Assert.ThrowsAsync<DomainRuleViolationException>(() => handler.Handle(
            new CreateAirportCommand("SCL", "Otro", "Otra", "Otro", "America/Santiago"),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_with_invalid_timezone_throws()
    {
        var handler = new CreateAirportCommandHandler(new InMemoryAirportRepository(), new NoOpUnitOfWork());

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(
            new CreateAirportCommand("SCL", "Arturo Merino Benitez", "Santiago", "Chile", "Not/A_Real_Zone"),
            CancellationToken.None));
    }
}
