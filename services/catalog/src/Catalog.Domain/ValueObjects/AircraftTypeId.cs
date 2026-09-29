namespace Catalog.Domain.ValueObjects;

public readonly record struct AircraftTypeId(Guid Value)
{
    public static AircraftTypeId New() => new(Guid.NewGuid());

    public override string ToString() => Value.ToString();
}
