using Catalog.Application.Common.ReadModels;
using Catalog.Application.Flights.SearchFlights;
using Catalog.Domain.Flights;

namespace Catalog.Application.Tests.Flights.SearchFlights;

public class SearchFlightsQueryHandlerTests
{
    [Fact]
    public async Task Handle_normalizes_airport_codes_before_querying_the_read_model()
    {
        var readModel = new CapturingFlightSearchReadModel();
        var handler = new SearchFlightsQueryHandler(readModel);

        await handler.Handle(
            new SearchFlightsQuery("scl", "lim", new DateOnly(2026, 3, 15)),
            CancellationToken.None);

        Assert.Equal("SCL", readModel.LastCriteria!.Origin);
        Assert.Equal("LIM", readModel.LastCriteria!.Destination);
    }

    [Fact]
    public async Task Handle_defaults_passenger_count_to_one()
    {
        var readModel = new CapturingFlightSearchReadModel();
        var handler = new SearchFlightsQueryHandler(readModel);

        await handler.Handle(
            new SearchFlightsQuery("SCL", "LIM", new DateOnly(2026, 3, 15)),
            CancellationToken.None);

        Assert.Equal(1, readModel.LastCriteria!.PassengerCount);
    }

    [Fact]
    public async Task Handle_returns_whatever_the_read_model_produces()
    {
        var expected = new FlightSearchResult(
            Guid.NewGuid(), "LA", "LA800", "SCL", "LIM",
            DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2),
            [new FareClassOption(CabinClass.Economy, 120, "USD", 150)]);
        var readModel = new CapturingFlightSearchReadModel(expected);
        var handler = new SearchFlightsQueryHandler(readModel);

        var results = await handler.Handle(
            new SearchFlightsQuery("SCL", "LIM", new DateOnly(2026, 3, 15)),
            CancellationToken.None);

        Assert.Same(expected, Assert.Single(results));
    }

    [Fact]
    public async Task Handle_with_same_origin_and_destination_throws()
    {
        var handler = new SearchFlightsQueryHandler(new CapturingFlightSearchReadModel());

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(
            new SearchFlightsQuery("SCL", "scl", new DateOnly(2026, 3, 15)),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_with_invalid_airport_code_throws()
    {
        var handler = new SearchFlightsQueryHandler(new CapturingFlightSearchReadModel());

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(
            new SearchFlightsQuery("SANTIAGO", "LIM", new DateOnly(2026, 3, 15)),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_with_non_positive_passenger_count_throws()
    {
        var handler = new SearchFlightsQueryHandler(new CapturingFlightSearchReadModel());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => handler.Handle(
            new SearchFlightsQuery("SCL", "LIM", new DateOnly(2026, 3, 15), PassengerCount: 0),
            CancellationToken.None));
    }
}
