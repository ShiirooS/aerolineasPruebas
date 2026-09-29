using Catalog.Domain.Common;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Flights;

public sealed class FareClass : Entity<FareClassId>
{
    public CabinClass Cabin { get; }
    public Money BasePrice { get; }
    public int AvailableSeats { get; private set; }

    // Constructor privado sin parámetros para EF Core: BasePrice es una
    // navegación owned (Money vía OwnsOne en la configuración de Flight), y EF
    // Core no soporta bindear navegaciones owned por constructor, solo columnas
    // escalares simples. Con este constructor disponible, EF materializa la fila
    // escribiendo los campos de respaldo directamente y nunca pasa por el
    // constructor de abajo.
#pragma warning disable CS8618
    private FareClass()
    {
    }
#pragma warning restore CS8618

    // El parámetro se llama "availableSeats" (no "totalSeats") para que coincida
    // con la propiedad AvailableSeats: EF Core hace binding de constructor por
    // nombre de propiedad al materializar filas.
    internal FareClass(FareClassId id, CabinClass cabin, Money basePrice, int availableSeats)
        : base(id)
    {
        if (availableSeats <= 0)
            throw new ArgumentOutOfRangeException(nameof(availableSeats), availableSeats, "La cabina debe tener al menos un asiento.");

        Cabin = cabin;
        BasePrice = basePrice;
        AvailableSeats = availableSeats;
    }

    internal bool TryReserveSeats(int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "La cantidad de asientos a reservar debe ser mayor a cero.");

        if (count > AvailableSeats)
            return false;

        AvailableSeats -= count;
        return true;
    }

    internal void ReleaseSeats(int count)
    {
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "La cantidad de asientos a liberar debe ser mayor a cero.");

        AvailableSeats += count;
    }
}
