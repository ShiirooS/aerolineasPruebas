using Catalog.Application.Common.Interfaces;

namespace Catalog.Application.Tests.TestDoubles;

internal sealed class NoOpUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveChangesCallCount++;
        return Task.CompletedTask;
    }
}
