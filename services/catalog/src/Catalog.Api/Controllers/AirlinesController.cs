using Catalog.Application.Airlines.CreateAirline;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/airlines")]
public sealed class AirlinesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateAirlineResult>> Create(CreateAirlineCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Created($"/api/airlines/{result.Code}", result);
    }
}
