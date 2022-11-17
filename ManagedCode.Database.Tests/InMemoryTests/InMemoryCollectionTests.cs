using System.Threading;
using ManagedCode.Database.Core.InMemory;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using System.Threading.Tasks;

namespace ManagedCode.Database.Tests.InMemoryTests;

public class InMemoryCollectionTests : BaseCollectionTests<int, InMemoryItem>
{
    private static volatile int _count;
    private InMemoryDatabase _database;

    protected override IDatabaseCollection<int, InMemoryItem> Collection =>
        _database.GetCollection<int, InMemoryItem>();

    protected override int GenerateId()
    {
        Interlocked.Increment(ref _count);
        return _count;
    }

    public override async Task InitializeAsync()
    {
        _database = new InMemoryDatabase();
        await _database.InitializeAsync();
    }
         
    public override async Task DisposeAsync()
    {
        await _database.DisposeAsync();
    }
}