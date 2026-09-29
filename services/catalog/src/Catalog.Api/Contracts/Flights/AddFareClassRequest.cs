using Catalog.Domain.Flights;

namespace Catalog.Api.Contracts.Flights;

public sealed record AddFareClassRequest(CabinClass Cabin, decimal Price, string Currency, int TotalSeats);
