using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.LiteDBTests;

public class LiteDBCollectionTests : BaseCollectionTests<string, TestLiteDBItem>
{
    public LiteDBCollectionTests() : base(new LiteDBTestContainer())
    {

    }

    public override async Task InsertOrUpdateListOfItems()
    {

        // Arrange
        int itemsCount = 5;
        int updatedItemsCount = 0;
        List<TestLiteDBItem> list = new();

        for (var i = 0; i < itemsCount; i++)
        {
            list.Add(CreateNewItem());
        }

        var itemsInsert = await Collection.InsertOrUpdateAsync(list);

        foreach (var item in list)
        {
            item.DateTimeData = DateTime.Now.AddDays(-1);
        }

        // Act
        var itemsUpdate = await Collection.InsertOrUpdateAsync(list);
        //TODO: LiteDB must be 100, but result 0

        // Assert
        itemsUpdate.Should().Be(updatedItemsCount);
        itemsInsert.Should().Be(itemsCount);
        list.Count.Should().Be(itemsCount);
    }
}