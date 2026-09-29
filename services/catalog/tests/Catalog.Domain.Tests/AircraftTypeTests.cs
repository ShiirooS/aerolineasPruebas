using Catalog.Domain.Aircraft;
using Catalog.Domain.Flights;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Tests;

public class AircraftTypeTests
{
    private static SeatMap CreateSeatMap() =>
        new(new Dictionary<CabinClass, int> { [CabinClass.Economy] = 150, [CabinClass.Business] = 20 });

    [Fact]
    public void Constructor_with_blank_model_throws()
    {
        Assert.Throws<ArgumentException>(() => new AircraftType(AircraftTypeId.New(), "   ", CreateSeatMap()));
    }

    [Fact]
    public void UpdateSeatMap_replaces_capacity()
    {
        var aircraftType = new AircraftType(AircraftTypeId.New(), "Airbus A320", CreateSeatMap());
        var largerSeatMap = new SeatMap(new Dictionary<CabinClass, int> { [CabinClass.Economy] = 180 });

        aircraftType.UpdateSeatMap(largerSeatMap);

        Assert.Equal(180, aircraftType.SeatMap.CapacityFor(CabinClass.Economy));
    }
}
