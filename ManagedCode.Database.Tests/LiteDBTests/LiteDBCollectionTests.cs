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
        int itemsCount = 5;
        List<TestLiteDBItem> items = new List<TestLiteDBItem>();

        for (int i = 0; i < itemsCount; i++)
        {
            items.Add(CreateNewItem());
        }
        
        var itemsInsert = await Collection.InsertOrUpdateAsync(items);

        for (int i = 0; i < itemsCount; i++)
        {
            items[i] = CreateNewItem(items[i].Id);
        }
        
        // Act
        var itemsUpdate = 0;

        foreach (var item in items)
        {
            var upsertResult = await Collection.InsertOrUpdateAsync(item);
            if (upsertResult != null)
                itemsUpdate++;
        }
        
        
        // Assert
        itemsUpdate.Should().Be(itemsCount);
        itemsInsert.Should().Be(itemsCount);
        items.Count.Should().Be(itemsCount);
    }
}