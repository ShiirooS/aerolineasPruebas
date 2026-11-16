using Catalog.Application.Airports;
using Catalog.Application.Airports.CreateAirport;
using Catalog.Application.Airports.GetAirport;
using Catalog.Application.Airports.ListAirports;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/airports")]
public sealed class AirportsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AirportResult>>> List(CancellationToken cancellationToken) =>
        Ok(await sender.Send(new ListAirportsQuery(), cancellationToken));

    [HttpGet("{code}")]
    public async Task<ActionResult<AirportResult>> Get(string code, CancellationToken cancellationToken) =>
        Ok(await sender.Send(new GetAirportQuery(code), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CreateAirportResult>> Create(CreateAirportCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Created($"/api/airports/{result.Code}", result);
    }
}
