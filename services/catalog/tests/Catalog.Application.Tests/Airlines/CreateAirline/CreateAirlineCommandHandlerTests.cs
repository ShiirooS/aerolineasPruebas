using Catalog.Application.Airlines.CreateAirline;
using Catalog.Application.Tests.TestDoubles;
using Catalog.Domain.Airlines;
using Catalog.Domain.Common;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Tests.Airlines.CreateAirline;

public class CreateAirlineCommandHandlerTests
{
    [Fact]
    public async Task Handle_with_new_code_creates_airline()
    {
        var repository = new InMemoryAirlineRepository();
        var handler = new CreateAirlineCommandHandler(repository, new NoOpUnitOfWork());

        var result = await handler.Handle(new CreateAirlineCommand("la", "LATAM Airlines"), CancellationToken.None);

        Assert.Equal("LA", result.Code);
    }

    [Fact]
    public async Task Handle_with_existing_code_throws()
    {
        var existing = new Airline(new AirlineCode("LA"), "Existente");
        var repository = new InMemoryAirlineRepository(existing);
        var handler = new CreateAirlineCommandHandler(repository, new NoOpUnitOfWork());

        await Assert.ThrowsAsync<DomainRuleViolationException>(() =>
            handler.Handle(new CreateAirlineCommand("LA", "Otra"), CancellationToken.None));
    }
}
