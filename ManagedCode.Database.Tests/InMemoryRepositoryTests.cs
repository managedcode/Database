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
    private InMemoryDataBase _db;

    public InMemoryDataBaseTests() 
    {
        _db = new InMemoryDataBase();
    }

    protected override IDBCollection<int, InMemoryItem> Repository => _db.GetCollection<int, InMemoryItem>();

    protected override int GenerateId()
    {
        _count++;
        return _count;
    }

    [Fact]
    public virtual async Task InitializeAsync()
    {
        await _db.InitializeAsync();
        await _db.InitializeAsync();
        await _db.InitializeAsync();
        await _db.InitializeAsync();
    }
}

