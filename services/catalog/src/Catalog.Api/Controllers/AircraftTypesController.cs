using Catalog.Application.Aircraft.CreateAircraftType;
using Catalog.Application.Aircraft.GetAircraftType;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/aircraft-types")]
public sealed class AircraftTypesController(ISender sender) : ControllerBase
{
    [HttpGet("{aircraftTypeId:guid}")]
    public async Task<ActionResult<AircraftTypeResult>> Get(Guid aircraftTypeId, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAircraftTypeQuery(aircraftTypeId), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CreateAircraftTypeResult>> Create(CreateAircraftTypeCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Created($"/api/aircraft-types/{result.AircraftTypeId}", result);
    }
}
