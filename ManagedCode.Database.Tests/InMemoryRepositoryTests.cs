using ManagedCode.Database.Core;
using ManagedCode.Database.Core.InMemory;
using ManagedCode.Database.Tests.Common;

namespace ManagedCode.Database.Tests;

public class InMemoryDataBaseTests : QueryableTests<int, InMemoryItem>
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

}

