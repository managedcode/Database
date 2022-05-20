using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Repository.AzureTable;
using ManagedCode.Repository.Core;
using ManagedCode.Repository.Tests.Common;
using Xunit;

namespace ManagedCode.Repository.Tests;

public class InMemoryRepositoryTests : BaseRepositoryTests<int, InMemoryItem>
{
    private static int _count;

    public InMemoryRepositoryTests() : base(new InMemoryRepository<int, InMemoryItem>())
    {
        Repository.InitializeAsync().Wait();
    }

    protected override int GenerateId()
    {
        _count++;
        return _count;
    }
}

public class InMemoryAzureRepositoryTests
{
    private static int _count;

    class TestRepo : InMemoryRepository<TableId, AzureTableItem>
    {
        
    }

    [Fact]
    public async Task IdTest()
    {
        var repo = new TestRepo();
        await repo.InsertOrUpdateAsync(new AzureTableItem { Id = new TableId("1","2") });
        var item = await repo.GetAsync(new TableId("1", "2"));
        item.Should().NotBeNull();
    }
}