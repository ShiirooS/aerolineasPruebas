using System.Collections;
using Catalog.Domain.Flights;

namespace Catalog.Domain.Aircraft;

/// <remarks>
/// No es un record a propósito: un record compara sus propiedades con Equals(),
/// y el Equals() de Dictionary es por referencia, no por contenido. Un record
/// acá daría una igualdad estructural falsa (dos SeatMap con las mismas cabinas
/// y capacidades compararían como distintos). Por eso la igualdad se implementa
/// a mano comparando las entradas ordenadas.
///
/// Implementa IEnumerable de solo lectura (no expone el Dictionary) para que
/// Infrastructure pueda recorrer todas las cabinas al serializar a JSON sin
/// que el dominio tenga que abrir su estado interno mutable.
/// </remarks>
public sealed class SeatMap : IEquatable<SeatMap>, IEnumerable<KeyValuePair<CabinClass, int>>
{
    private readonly Dictionary<CabinClass, int> _capacityByCabin;

    public SeatMap(IReadOnlyDictionary<CabinClass, int> capacityByCabin)
    {
        if (capacityByCabin is null || capacityByCabin.Count == 0)
            throw new ArgumentException("El mapa de asientos debe definir al menos una cabina.", nameof(capacityByCabin));

        foreach (var (cabin, capacity) in capacityByCabin)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacityByCabin), capacity, $"La capacidad de la cabina {cabin} debe ser mayor a cero.");
        }

        _capacityByCabin = new Dictionary<CabinClass, int>(capacityByCabin);
    }

    public int TotalSeats => _capacityByCabin.Values.Sum();

    public int CapacityFor(CabinClass cabin) =>
        _capacityByCabin.GetValueOrDefault(cabin);

    public bool Equals(SeatMap? other)
    {
        if (other is null)
            return false;

        return _capacityByCabin.Count == other._capacityByCabin.Count
            && _capacityByCabin.All(entry =>
                other._capacityByCabin.TryGetValue(entry.Key, out var otherCapacity) && entry.Value == otherCapacity);
    }

    public override bool Equals(object? obj) => Equals(obj as SeatMap);

    public override int GetHashCode() =>
        _capacityByCabin
            .OrderBy(entry => entry.Key)
            .Aggregate(0, (hash, entry) => HashCode.Combine(hash, entry.Key, entry.Value));

    public IEnumerator<KeyValuePair<CabinClass, int>> GetEnumerator() => _capacityByCabin.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
