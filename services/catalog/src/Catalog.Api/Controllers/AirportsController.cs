using Catalog.Application.Airports.CreateAirport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/airports")]
public sealed class AirportsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateAirportResult>> Create(CreateAirportCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Created($"/api/airports/{result.Code}", result);
    }
}
