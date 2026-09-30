using Catalog.Domain.Common;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Airports;

public sealed class Airport : Entity<AirportCode>
{
    public string Name { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }
    public TimeZoneInfo TimeZone { get; private set; }

    // El parámetro se llama "id", no "code", a propósito: EF Core hace binding
    // de constructor por nombre contra las propiedades del modelo (acá, la
    // heredada Entity<TId>.Id), no por el nombre del tipo. Un parámetro "code"
    // rompe la creación de instancias en tiempo de consulta.
    public Airport(AirportCode id, string name, string city, string country, TimeZoneInfo timeZone)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del aeropuerto es obligatorio.", nameof(name));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("La ciudad del aeropuerto es obligatoria.", nameof(city));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("El país del aeropuerto es obligatorio.", nameof(country));

        Name = name;
        City = city;
        Country = country;
        TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
    }

    /// <summary>
    /// Instante dado, expresado con el offset local vigente en este aeropuerto
    /// en ese momento (respeta DST).
    /// </summary>
    public DateTimeOffset ToLocalTime(DateTimeOffset instant) =>
        TimeZoneInfo.ConvertTime(instant, TimeZone);

    /// <summary>
    /// Rango [inicio, fin) en UTC que cubre el día calendario <paramref name="localDate"/>
    /// según el reloj de este aeropuerto. Un pasajero que busca "vuelos del 10/10"
    /// piensa en el día local del aeropuerto de origen, no en el día UTC.
    /// </summary>
    public (DateTimeOffset StartUtc, DateTimeOffset EndUtc) LocalDayBoundsUtc(DateOnly localDate) =>
        (StartOfLocalDayUtc(localDate), StartOfLocalDayUtc(localDate.AddDays(1)));

    private DateTimeOffset StartOfLocalDayUtc(DateOnly localDate)
    {
        var localMidnight = localDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);

        // Medianoche ambigua (el reloj retrocede a las 00:00 y esa hora ocurre dos
        // veces): el día empieza en la PRIMERA ocurrencia, que es la del offset mayor.
        // Medianoche inválida (el reloj salta de 23:59 a 01:00, como en Chile al
        // entrar en horario de verano): GetUtcOffset devuelve el offset estándar, y
        // medianoche con offset estándar cae justo en el instante del salto, que es
        // el primer momento real del día. Ambos casos están cubiertos en AirportTests.
        var offset = TimeZone.IsAmbiguousTime(localMidnight)
            ? TimeZone.GetAmbiguousTimeOffsets(localMidnight).Max()
            : TimeZone.GetUtcOffset(localMidnight);

        return new DateTimeOffset(localMidnight, offset).ToUniversalTime();
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("El nombre del aeropuerto es obligatorio.", nameof(newName));

        Name = newName;
    }
}
