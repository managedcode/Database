using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using Xunit;

namespace ManagedCode.Database.Tests.BaseTests;

// TODO: rename
public abstract class BaseTests<TId, TItem> : IAsyncLifetime where TItem : IBaseItem<TId>, new()
{
    private readonly ITestContainer<TId, TItem> _testContainer;

    protected BaseTests(ITestContainer<TId, TItem> testContainer)
    {
        _testContainer = testContainer;
    }

    protected IDatabaseCollection<TId, TItem> Collection => _testContainer.Collection;

    protected TId GenerateId()
    {
        return _testContainer.GenerateId();
    }

    public async Task InitializeAsync()
    {
        await _testContainer.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _testContainer.DisposeAsync();
    }
}