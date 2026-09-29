using Catalog.Domain.Common;

namespace Catalog.Domain.ValueObjects;

public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "El monto no puede ser negativo.");

        var normalizedCurrency = currency?.Trim().ToUpperInvariant() ?? string.Empty;

        if (normalizedCurrency.Length != 3 || !normalizedCurrency.All(c => c is >= 'A' and <= 'Z'))
            throw new ArgumentException($"'{currency}' no es un código de moneda ISO 4217 válido.", nameof(currency));

        Amount = amount;
        Currency = normalizedCurrency;
    }

    public static Money Zero(string currency) => new(0, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainRuleViolationException($"No se pueden operar montos en monedas distintas ({Currency} vs {other.Currency}).");
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
