using Catalog.Domain.Flights;

namespace Catalog.Application.Aircraft.GetAircraftType;

public sealed record AircraftTypeResult(Guid AircraftTypeId, string Model, IReadOnlyDictionary<CabinClass, int> SeatMap);
