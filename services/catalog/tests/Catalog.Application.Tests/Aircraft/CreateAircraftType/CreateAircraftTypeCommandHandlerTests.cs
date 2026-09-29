using Catalog.Application.Aircraft.CreateAircraftType;
using Catalog.Application.Tests.Flights.AddFareClassToFlight;
using Catalog.Application.Tests.TestDoubles;
using Catalog.Domain.Flights;

namespace Catalog.Application.Tests.Aircraft.CreateAircraftType;

public class CreateAircraftTypeCommandHandlerTests
{
    [Fact]
    public async Task Handle_creates_aircraft_type_with_seat_map()
    {
        var repository = new InMemoryAircraftTypeRepository();
        var unitOfWork = new NoOpUnitOfWork();
        var handler = new CreateAircraftTypeCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CreateAircraftTypeCommand("Airbus A320", new Dictionary<CabinClass, int> { [CabinClass.Economy] = 150 }),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.AircraftTypeId);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_with_empty_seat_map_throws()
    {
        var handler = new CreateAircraftTypeCommandHandler(new InMemoryAircraftTypeRepository(), new NoOpUnitOfWork());

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(
            new CreateAircraftTypeCommand("Airbus A320", new Dictionary<CabinClass, int>()),
            CancellationToken.None));
    }
}
