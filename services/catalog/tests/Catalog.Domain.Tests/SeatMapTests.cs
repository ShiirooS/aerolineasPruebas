using Catalog.Domain.Aircraft;
using Catalog.Domain.Flights;

namespace Catalog.Domain.Tests;

public class SeatMapTests
{
    [Fact]
    public void CapacityFor_returns_configured_capacity()
    {
        var seatMap = new SeatMap(new Dictionary<CabinClass, int>
        {
            [CabinClass.Economy] = 150,
            [CabinClass.Business] = 20
        });

        Assert.Equal(150, seatMap.CapacityFor(CabinClass.Economy));
        Assert.Equal(20, seatMap.CapacityFor(CabinClass.Business));
    }

    [Fact]
    public void CapacityFor_missing_cabin_returns_zero()
    {
        var seatMap = new SeatMap(new Dictionary<CabinClass, int> { [CabinClass.Economy] = 150 });

        Assert.Equal(0, seatMap.CapacityFor(CabinClass.First));
    }

    [Fact]
    public void TotalSeats_sums_all_cabins()
    {
        var seatMap = new SeatMap(new Dictionary<CabinClass, int>
        {
            [CabinClass.Economy] = 150,
            [CabinClass.Business] = 20
        });

        Assert.Equal(170, seatMap.TotalSeats);
    }

    [Fact]
    public void Constructor_with_empty_map_throws()
    {
        Assert.Throws<ArgumentException>(() => new SeatMap(new Dictionary<CabinClass, int>()));
    }

    [Fact]
    public void Constructor_with_non_positive_capacity_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SeatMap(new Dictionary<CabinClass, int> { [CabinClass.Economy] = 0 }));
    }

    [Fact]
    public void Equals_compares_by_content_not_reference()
    {
        var a = new SeatMap(new Dictionary<CabinClass, int> { [CabinClass.Economy] = 150, [CabinClass.Business] = 20 });
        var b = new SeatMap(new Dictionary<CabinClass, int> { [CabinClass.Business] = 20, [CabinClass.Economy] = 150 });

        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }
}
