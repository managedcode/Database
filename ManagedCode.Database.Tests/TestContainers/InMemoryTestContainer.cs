using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.InMemory;
using ManagedCode.Database.Tests.Common;

namespace ManagedCode.Database.Tests.TestContainers;

public class InMemoryTestContainer
{
    private static volatile int _count;
    private InMemoryDatabase _database;

    public IDatabaseCollection<int, InMemoryItem> Collection => _database.GetCollection<int, InMemoryItem>();

    public int GenerateId()
    {
        Interlocked.Increment(ref _count);
        return _count;
    }

    public async Task InitializeAsync()
    {
        _database = new InMemoryDatabase();
        await _database.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _database.DisposeAsync();
    }
}