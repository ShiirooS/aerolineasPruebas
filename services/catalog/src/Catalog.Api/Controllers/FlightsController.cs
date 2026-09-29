using Catalog.Api.Contracts.Flights;
using Catalog.Application.Common.ReadModels;
using Catalog.Application.Flights.AddFareClassToFlight;
using Catalog.Application.Flights.CreateFlight;
using Catalog.Application.Flights.SearchFlights;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/flights")]
public sealed class FlightsController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FlightSearchResult>>> Search(
        [FromQuery] SearchFlightsQuery query,
        CancellationToken cancellationToken)
    {
        var results = await sender.Send(query, cancellationToken);
        return Ok(results);
    }

    [HttpPost]
    public async Task<ActionResult<CreateFlightResult>> Create(CreateFlightCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Created($"/api/flights/{result.FlightId}", result);
    }

    [HttpPost("{flightId:guid}/fare-classes")]
    public async Task<ActionResult<AddFareClassToFlightResult>> AddFareClass(
        Guid flightId,
        AddFareClassRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddFareClassToFlightCommand(flightId, request.Cabin, request.Price, request.Currency, request.TotalSeats);
        var result = await sender.Send(command, cancellationToken);

        return Created($"/api/flights/{flightId}", result);
    }
}
