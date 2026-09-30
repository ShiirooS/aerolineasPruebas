using Catalog.Application.Airlines.CreateAirline;
using Catalog.Application.Airlines.GetAirline;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/airlines")]
public sealed class AirlinesController(ISender sender) : ControllerBase
{
    [HttpGet("{code}")]
    public async Task<ActionResult<AirlineResult>> Get(string code, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAirlineQuery(code), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CreateAirlineResult>> Create(CreateAirlineCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Created($"/api/airlines/{result.Code}", result);
    }
}
