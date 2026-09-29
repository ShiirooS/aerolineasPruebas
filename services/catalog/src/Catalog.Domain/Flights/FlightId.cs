namespace Catalog.Domain.Flights;

public readonly record struct FlightId(Guid Value)
{
    public static FlightId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
