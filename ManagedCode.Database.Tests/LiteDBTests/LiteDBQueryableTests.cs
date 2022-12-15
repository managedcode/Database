using System;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.LiteDBTests;

public class LiteDBQueryableTests : BaseQueryableTests<string, TestLiteDBItem>
{
    public LiteDBQueryableTests() : base(new LiteDBTestContainer())
    {
    }

    public override async Task OrderBy_AfterIOrdered_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult =
           await Collection.Query
            .OrderBy(o => o.IntData)
            .OrderBy(o => o.StringData)
            .ToListAsync();

        // Assert
        itemsResult.Should().NotBeNull();
        itemsResult.Count.Should().Be(itemsCountToInsert);
    }
}