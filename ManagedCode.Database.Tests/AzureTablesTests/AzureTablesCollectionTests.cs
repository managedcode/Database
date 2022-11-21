using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.AzureTables;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.AzureTablesTests;

public class AzureTablesCollectionTests : BaseCollectionTests<TableId, TestAzureTablesItem>
{
    public AzureTablesCollectionTests() : base(new AzureTablesTestContainer())
    {
    }

/*    public override async Task UpdateItem_WhenItem_DoesntExists()
    {
        var baseMethod = () => base.UpdateItem_WhenItem_DoesntExists();
        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
    }

    public override async Task InsertItem_WhenItemExist()
    {
        var baseMethod = () => base.InsertItem_WhenItemExist();
        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
    }*/

    public override async Task DeleteItemById_WhenItemDoesntExists()
    {
        var item = CreateNewItem();

        var deleteAction = async () => await Collection.DeleteAsync(item.Id);

        item.Should().NotBeNull();
        await deleteAction.Should().ThrowExactlyAsync<Exception>();
    }

    public override async Task DeleteListOfItemsById_WhenItemsDontExist()
    {
        int itemsCount = 5;
        List<TestAzureTablesItem> list = new();

        for (var i = 0; i < itemsCount; i++)
        {
            list.Add(CreateNewItem());
        }

        var ids = list.Select(item => item.Id);

        var deletedItemsAction = async () => await Collection.DeleteAsync(ids);

        // await deletedItemsAction.Should().ThrowExactlyAsync<StorageException>();
    }

    public override async Task DeleteListOfItems_WhenItemsDontExist()
    {
        int itemsCount = 5;
        List<TestAzureTablesItem> list = new();

        for (var i = 0; i < itemsCount; i++)
        {
            list.Add(CreateNewItem());
        }

        var deletedItemsAction = async () => await Collection.DeleteAsync(list);

        // await deletedItemsAction.Should().ThrowExactlyAsync<StorageException>();
        list.Count.Should().Be(itemsCount);
    }
}