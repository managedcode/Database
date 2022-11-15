using ManagedCode.Database.Core;
using ManagedCode.Database.Core.InMemory;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using System.Threading.Tasks;

namespace ManagedCode.Database.Tests.InMemoryTests;

public class InMemoryQueryableTests : BaseQueryableTests<int, InMemoryItem>
{
    private static volatile int _count;
    private InMemoryDataBase _database;

    public InMemoryQueryableTests()
    {
        _database = new InMemoryDataBase();
    }

    protected override IDBCollection<int, InMemoryItem> Collection => _database.GetCollection<int, InMemoryItem>();

    protected override int GenerateId()
    {
        _count++;
        return _count;
    }
    
    public override async Task InitializeAsync() =>
            await _database.InitializeAsync();

    public override async Task DisposeAsync() =>
        await _database.DeleteAsync();

}

