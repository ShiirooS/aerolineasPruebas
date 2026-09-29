namespace Catalog.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"No se encontró {entityName} con id '{key}'.")
    {
    }
}
