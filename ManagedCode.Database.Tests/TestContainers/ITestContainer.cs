using ManagedCode.Database.Core;
using Xunit;

namespace ManagedCode.Database.Tests.TestContainers;

public interface ITestContainer<TId, TItem> : IAsyncLifetime where TItem : IItem<TId>
{
    public IDatabaseCollection<TId, TItem> Collection { get; }

    public TId GenerateId();
}