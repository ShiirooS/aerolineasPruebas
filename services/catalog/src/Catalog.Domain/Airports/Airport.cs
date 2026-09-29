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

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("El nombre del aeropuerto es obligatorio.", nameof(newName));

        Name = newName;
    }
}
