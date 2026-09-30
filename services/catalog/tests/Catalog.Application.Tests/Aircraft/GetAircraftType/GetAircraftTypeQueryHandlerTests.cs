using Catalog.Application.Aircraft.GetAircraftType;
using Catalog.Application.Common.Exceptions;
using Catalog.Application.Tests.Flights.AddFareClassToFlight;
using Catalog.Domain.Aircraft;
using Catalog.Domain.Flights;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Tests.Aircraft.GetAircraftType;

public class GetAircraftTypeQueryHandlerTests
{
    [Fact]
    public async Task Handle_with_existing_id_returns_seat_map()
    {
        var a320 = new AircraftType(
            AircraftTypeId.New(), "Airbus A320",
            new SeatMap(new Dictionary<CabinClass, int> { [CabinClass.Economy] = 150, [CabinClass.Business] = 12 }));
        var handler = new GetAircraftTypeQueryHandler(new InMemoryAircraftTypeRepository(a320));

        var result = await handler.Handle(new GetAircraftTypeQuery(a320.Id.Value), CancellationToken.None);

        Assert.Equal("Airbus A320", result.Model);
        Assert.Equal(12, result.SeatMap[CabinClass.Business]);
    }

    [Fact]
    public async Task Handle_with_unknown_id_throws_not_found()
    {
        var handler = new GetAircraftTypeQueryHandler(new InMemoryAircraftTypeRepository());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetAircraftTypeQuery(Guid.NewGuid()), CancellationToken.None));
    }
}
