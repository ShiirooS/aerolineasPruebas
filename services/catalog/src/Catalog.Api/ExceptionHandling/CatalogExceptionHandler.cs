using Catalog.Application.Common.Exceptions;
using Catalog.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.ExceptionHandling;

public sealed class CatalogExceptionHandler(ILogger<CatalogExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // Deliberadamente NO se mapea InvalidOperationException de forma genérica acá:
        // el propio framework (p. ej. una dependencia sin registrar en el contenedor DI)
        // también la tira, y confundirla con una regla de negocio real esconde bugs de
        // configuración detrás de un 409 que parece "todo más o menos bien".
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            SeatCapacityExceededException => (StatusCodes.Status409Conflict, "Capacidad excedida"),
            DomainRuleViolationException => (StatusCodes.Status409Conflict, "Regla de negocio violada"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno"),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Error no controlado procesando {Path}", httpContext.Request.Path);

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        }, cancellationToken);

        return true;
    }
}
