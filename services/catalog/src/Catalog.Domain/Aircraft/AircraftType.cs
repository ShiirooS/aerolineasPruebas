using Catalog.Domain.Common;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Aircraft;

public sealed class AircraftType : Entity<AircraftTypeId>
{
    public string Model { get; private set; }
    public SeatMap SeatMap { get; private set; }

    public AircraftType(AircraftTypeId id, string model, SeatMap seatMap)
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("El modelo de la aeronave es obligatorio.", nameof(model));

        Model = model;
        SeatMap = seatMap ?? throw new ArgumentNullException(nameof(seatMap));
    }

    public void Rename(string newModel)
    {
        if (string.IsNullOrWhiteSpace(newModel))
            throw new ArgumentException("El modelo de la aeronave es obligatorio.", nameof(newModel));

        Model = newModel;
    }

    public void UpdateSeatMap(SeatMap seatMap)
    {
        SeatMap = seatMap ?? throw new ArgumentNullException(nameof(seatMap));
    }
}
