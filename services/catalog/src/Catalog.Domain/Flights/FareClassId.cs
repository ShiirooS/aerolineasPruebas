namespace Catalog.Domain.Flights;

public readonly record struct FareClassId(Guid Value)
{
    public static FareClassId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
