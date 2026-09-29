using Catalog.Application.Aircraft.CreateAircraftType;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/aircraft-types")]
public sealed class AircraftTypesController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateAircraftTypeResult>> Create(CreateAircraftTypeCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Created($"/api/aircraft-types/{result.AircraftTypeId}", result);
    }
}
