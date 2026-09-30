using Catalog.Application.Airports.GetAirport;
using Catalog.Application.Common.Exceptions;
using Catalog.Application.Tests.TestDoubles;
using Catalog.Domain.Airports;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Tests.Airports.GetAirport;

public class GetAirportQueryHandlerTests
{
    [Fact]
    public async Task Handle_with_existing_code_returns_airport()
    {
        var lima = new Airport(new AirportCode("LIM"), "Jorge Chavez", "Lima", "Peru", TimeZoneInfo.FindSystemTimeZoneById("America/Lima"));
        var handler = new GetAirportQueryHandler(new InMemoryAirportRepository(lima));

        var result = await handler.Handle(new GetAirportQuery("lim"), CancellationToken.None);

        Assert.Equal("LIM", result.Code);
        Assert.Equal("America/Lima", result.TimeZoneId);
    }

    [Fact]
    public async Task Handle_with_unknown_code_throws_not_found()
    {
        var handler = new GetAirportQueryHandler(new InMemoryAirportRepository());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetAirportQuery("LIM"), CancellationToken.None));
    }
}
