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

    public override async Task InsertItems_WhenOneItemAlreadyExists()
    {
        var baseMethod = () => base.InsertItems_WhenOneItemAlreadyExists();
        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
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

    public override async Task DeleteAll_WhenNoItems()
    {
        var baseMethod = () => base.DeleteAll_WhenNoItems();
        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
    }
}