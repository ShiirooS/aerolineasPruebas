namespace Catalog.Domain.Common;

/// <remarks>
/// Existe para que "el dominio rechazó una operación por una regla de negocio" sea
/// distinguible de InvalidOperationException genéricas que tira el propio framework
/// (por ejemplo, un servicio no registrado en DI). Si el dominio usara
/// InvalidOperationException para esto, un handler de excepciones en la capa Api no
/// podría diferenciar "el vuelo ya tiene esa cabina" de "hay un bug de configuración",
/// y terminaría devolviendo el mismo código HTTP para ambos casos.
/// </remarks>
public sealed class DomainRuleViolationException : Exception
{
    public DomainRuleViolationException(string message) : base(message)
    {
    }
}
