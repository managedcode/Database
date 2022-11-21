using Azure;
using FluentAssertions;
using ManagedCode.Database.AzureTables;
using ManagedCode.Database.Core.Exceptions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ManagedCode.Database.Tests.AzureTablesTests;

public class AzureTablesCollectionTests : BaseCollectionTests<TableId, TestAzureTablesItem>
{
    public AzureTablesCollectionTests() : base(new AzureTablesTestContainer())
    {
    }

    public override async Task UpdateItem_WhenItemDoesntExists()
    {
        var baseMethod = () => base.UpdateItem_WhenItemDoesntExists();
        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
    }

    public override async Task DeleteListOfItemsById()
    {
        var baseMethod = () => base.DeleteListOfItemsById();
        await baseMethod.Should().ThrowExactlyAsync<NotSupportedException>();
    }

    public override async Task DeleteListOfItemsById_WhenItemsDontExist()
    {
        var baseMethod = () => base.DeleteListOfItemsById_WhenItemsDontExist();
        await baseMethod.Should().ThrowExactlyAsync<NotSupportedException>();
    }

    public override async Task DeleteListOfItems_WhenItemsDontExist()
    {
        var baseMethod = () => base.DeleteListOfItems_WhenItemsDontExist();
        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
    }

    public override async Task DeleteAll()
    {
        // Arrange
        int itemsCount = 5;
        List<TestAzureTablesItem> list = new();

        for (var i = 0; i < itemsCount; i++)
        {
            list.Add(CreateNewItem());
        }

        await Collection.InsertAsync(list);

        // Act
        var deletedItems = await Collection.DeleteCollectionAsync();
        var countAction = async () => await Collection.CountAsync();

        // Assert
        deletedItems.Should().BeTrue();
        await countAction.Should().ThrowExactlyAsync<DatabaseException>();
    }
}