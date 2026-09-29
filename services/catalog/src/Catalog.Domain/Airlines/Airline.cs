using Catalog.Domain.Common;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Airlines;

public sealed class Airline : Entity<AirlineCode>
{
    public string Name { get; private set; }

    // "id", no "code": ver el comentario equivalente en Airport sobre el
    // binding de constructor de EF Core por nombre de propiedad.
    public Airline(AirlineCode id, string name)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la aerolínea es obligatorio.", nameof(name));

        Name = name;
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("El nombre de la aerolínea es obligatorio.", nameof(newName));

        Name = newName;
    }
}
