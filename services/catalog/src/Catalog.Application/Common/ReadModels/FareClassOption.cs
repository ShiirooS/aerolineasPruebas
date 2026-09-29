using Catalog.Domain.Flights;

namespace Catalog.Application.Common.ReadModels;

public sealed record FareClassOption(
    CabinClass Cabin,
    decimal Price,
    string Currency,
    int AvailableSeats);
