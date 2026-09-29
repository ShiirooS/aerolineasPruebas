using Catalog.Domain.Common;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Add_with_same_currency_sums_amounts()
    {
        var total = new Money(100, "USD").Add(new Money(50, "USD"));

        Assert.Equal(new Money(150, "USD"), total);
    }

    [Fact]
    public void Add_with_different_currencies_throws()
    {
        Assert.Throws<DomainRuleViolationException>(() =>
            new Money(100, "USD").Add(new Money(50, "CLP")));
    }

    [Fact]
    public void Constructor_with_negative_amount_throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Money(-1, "USD"));
    }

    [Theory]
    [InlineData("us")]
    [InlineData("US1")]
    [InlineData("")]
    public void Constructor_with_invalid_currency_code_throws(string currency)
    {
        Assert.Throws<ArgumentException>(() => new Money(10, currency));
    }
}
