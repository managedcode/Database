using ManagedCode.Database.Core;
using ManagedCode.Database.Core.InMemory;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using System.Threading.Tasks;

namespace ManagedCode.Database.Tests.InMemoryTests;

public class InMemoryDataBaseTests : BaseRepositoryQueryableTests<int, InMemoryItem>
{
    private static volatile int _count;
    private InMemoryDataBase _databaseb;

    public InMemoryDataBaseTests()
    {
        _databaseb = new InMemoryDataBase();
    }

    protected override IDBCollection<int, InMemoryItem> Collection => _databaseb.GetCollection<int, InMemoryItem>();

    protected override int GenerateId()
    {
        _count++;
        return _count;
    }

    public override void Dispose()
    {
        _databaseb.Dispose();
    }

    public override async Task InitializeAsync() =>
            await _databaseb.InitializeAsync();

    public override async Task DisposeAsync() =>
        await _databaseb.DisposeAsync();

}

