namespace Catalog.Domain.ValueObjects;

public sealed record AirportCode
{
    public string Value { get; }

    public AirportCode(string value)
    {
        var normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;

        if (normalized.Length != 3 || !normalized.All(c => c is >= 'A' and <= 'Z'))
            throw new ArgumentException($"'{value}' no es un código IATA de aeropuerto válido (3 letras).", nameof(value));

        Value = normalized;
    }

    public override string ToString() => Value;
}
