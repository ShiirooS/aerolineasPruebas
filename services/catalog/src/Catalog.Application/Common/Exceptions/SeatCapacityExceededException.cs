using Catalog.Domain.Flights;

namespace Catalog.Application.Common.Exceptions;

public sealed class SeatCapacityExceededException : Exception
{
    public SeatCapacityExceededException(CabinClass cabin, int requestedSeats, int availableCapacity)
        : base($"La cabina {cabin} pidió {requestedSeats} asientos pero la aeronave solo tiene capacidad para {availableCapacity}.")
    {
    }
}
