using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.AzureTables;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Exceptions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.AzureTablesTests;

public class AzureTablesCollectionTests : BaseCollectionTests<TableId, TestAzureTablesItem>
{
    private readonly AzureTablesTestContainer _azureTablesContainer;

    public AzureTablesCollectionTests()
    {
        _azureTablesContainer = new AzureTablesTestContainer();
    }


    protected override IDatabaseCollection<TableId, TestAzureTablesItem> Collection =>
        _azureTablesContainer.GetCollection();

    protected override TableId GenerateId()
    {
        return _azureTablesContainer.GenerateId();
    }

    public override async Task DisposeAsync()
    {
        await _azureTablesContainer.DisposeAsync();
    }

    public override async Task InitializeAsync()
    {
        await _azureTablesContainer.InitializeAsync();
    }

    public override async Task UpdateListOfItems_WhenOnlyOneItemUpdated()
    {
        List<TestAzureTablesItem> list = new();

        var id = GenerateId();

        list.Add(CreateNewItem(id));
        for (var i = 0; i < 9; i++)
        {
            list.Add(CreateNewItem());
        }

        var items = await Collection.InsertAsync(list);
        list.Clear();

        list.Add(CreateNewItem(id));

        //  ᓚᘏᗢ It throws exception if item with that Id doesn't exists
        var updatedItems = await Collection.UpdateAsync(list);

        list.Count.Should().Be(1);
        items.Should().Be(10);
        updatedItems.Should().Be(1);
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

    public override async Task InsertItems_WhenOneItemAlreadyExists()
    {
        var baseMethod = () => base.InsertItems_WhenOneItemAlreadyExists();
        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
    }

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