using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Tests;

public class AirportCodeTests
{
    [Fact]
    public void Constructor_normalizes_lowercase_input()
    {
        var code = new AirportCode("scl");

        Assert.Equal("SCL", code.Value);
    }

    [Theory]
    [InlineData("SC")]
    [InlineData("SCLX")]
    [InlineData("SC1")]
    [InlineData("")]
    public void Constructor_with_invalid_code_throws(string value)
    {
        Assert.Throws<ArgumentException>(() => new AirportCode(value));
    }

    [Fact]
    public void Equality_is_by_value()
    {
        Assert.Equal(new AirportCode("SCL"), new AirportCode("scl"));
    }
}
