namespace Catalog.Domain.ValueObjects;

public sealed record FlightNumber
{
    public AirlineCode Airline { get; }
    public int Number { get; }

    public FlightNumber(AirlineCode airline, int number)
    {
        if (number is <= 0 or > 9999)
            throw new ArgumentOutOfRangeException(nameof(number), number, "El número de vuelo debe estar entre 1 y 9999.");

        Airline = airline;
        Number = number;
    }

    public override string ToString() => $"{Airline}{Number}";
}
