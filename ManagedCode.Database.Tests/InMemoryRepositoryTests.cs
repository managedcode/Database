using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.AzureTable;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.InMemory;
using ManagedCode.Database.Tests.Common;
using Xunit;

namespace ManagedCode.Database.Tests;

public class InMemoryDataBaseTests : BaseRepositoryTests<int, InMemoryItem>
{
    private static volatile int _count;
    private InMemoryDataBase _databaseb;

    public InMemoryDataBaseTests() 
    {
        _databaseb = new InMemoryDataBase();
    }

    protected override IDBCollection<int, InMemoryItem> Collection => _databaseb.GetCollection<int, InMemoryItem>();

    protected override ValueTask DeleteAllData()
    {
        throw new System.NotImplementedException();
    }

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

