using System.Threading;
using ManagedCode.Database.Core.InMemory;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using System.Threading.Tasks;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.InMemoryTests;

public class InMemoryCollectionTests : BaseCollectionTests<int, InMemoryItem>
{
    private readonly InMemoryTestContainer _testContainer;

    public InMemoryCollectionTests()
    {
        _testContainer = new InMemoryTestContainer();
    }

    protected override IDatabaseCollection<int, InMemoryItem> Collection => _testContainer.Collection;

    protected override int GenerateId()
    {
        return _testContainer.GenerateId();
    }

    public override async Task InitializeAsync()
    {
        await _testContainer.InitializeAsync();
    }

    public override async Task DisposeAsync()
    {
        await _testContainer.DisposeAsync();
    }
}