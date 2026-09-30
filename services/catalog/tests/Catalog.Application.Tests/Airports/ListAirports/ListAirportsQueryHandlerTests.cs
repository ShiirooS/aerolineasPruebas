using Catalog.Application.Airports.ListAirports;
using Catalog.Application.Tests.TestDoubles;
using Catalog.Domain.Airports;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Tests.Airports.ListAirports;

public class ListAirportsQueryHandlerTests
{
    [Fact]
    public async Task Handle_returns_airports_ordered_by_code()
    {
        var handler = new ListAirportsQueryHandler(new InMemoryAirportRepository(
            new Airport(new AirportCode("SCL"), "Arturo Merino Benitez", "Santiago", "Chile", TimeZoneInfo.Utc),
            new Airport(new AirportCode("LIM"), "Jorge Chavez", "Lima", "Peru", TimeZoneInfo.Utc)));

        var result = await handler.Handle(new ListAirportsQuery(), CancellationToken.None);

        Assert.Equal(["LIM", "SCL"], result.Select(a => a.Code));
    }

    [Fact]
    public async Task Handle_without_airports_returns_empty_list()
    {
        var handler = new ListAirportsQueryHandler(new InMemoryAirportRepository());

        var result = await handler.Handle(new ListAirportsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }
}
