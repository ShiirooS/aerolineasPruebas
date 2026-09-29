namespace Catalog.Domain.ValueObjects;

public sealed record AirlineCode
{
    public string Value { get; }

    public AirlineCode(string value)
    {
        var normalized = value?.Trim().ToUpperInvariant() ?? string.Empty;

        if (normalized.Length is < 2 or > 3 || !normalized.All(char.IsAsciiLetterOrDigit))
            throw new ArgumentException($"'{value}' no es un código IATA de aerolínea válido (2-3 caracteres alfanuméricos).", nameof(value));

        Value = normalized;
    }

    public override string ToString() => Value;
}
