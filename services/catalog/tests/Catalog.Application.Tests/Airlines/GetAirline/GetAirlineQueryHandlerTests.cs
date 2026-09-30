using Catalog.Application.Airlines.GetAirline;
using Catalog.Application.Common.Exceptions;
using Catalog.Application.Tests.TestDoubles;
using Catalog.Domain.Airlines;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Tests.Airlines.GetAirline;

public class GetAirlineQueryHandlerTests
{
    [Fact]
    public async Task Handle_with_existing_code_returns_airline()
    {
        var handler = new GetAirlineQueryHandler(new InMemoryAirlineRepository(new Airline(new AirlineCode("LA"), "LATAM")));

        var result = await handler.Handle(new GetAirlineQuery("LA"), CancellationToken.None);

        Assert.Equal(new AirlineResult("LA", "LATAM"), result);
    }

    [Fact]
    public async Task Handle_with_unknown_code_throws_not_found()
    {
        var handler = new GetAirlineQueryHandler(new InMemoryAirlineRepository());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetAirlineQuery("LA"), CancellationToken.None));
    }
}
