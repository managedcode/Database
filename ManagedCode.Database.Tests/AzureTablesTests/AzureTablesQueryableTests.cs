using FluentAssertions;
using ManagedCode.Database.AzureTables;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ManagedCode.Database.Tests.AzureTablesTests;

public class AzureTablesQueryableTests : BaseQueryableTests<TableId, TestAzureTablesItem>
{
    public AzureTablesQueryableTests() : base(new AzureTablesTestContainer())
    {
    }

    [Fact]
    public override async Task Where_TakeNull_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;


        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = () => Collection.Query
            .Where(null)
            .ToListAsync();

        // Assert
        await itemsResult
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }
}