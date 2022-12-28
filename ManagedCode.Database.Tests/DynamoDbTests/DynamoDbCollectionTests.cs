using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using FluentAssertions;
using ManagedCode.Database.Core.Exceptions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ManagedCode.Database.Tests.DynamoDbTests;

public class DynamoDbCollectionTests : BaseCollectionTests<string, TestDynamoDbItem>
{
    public DynamoDbCollectionTests() : base(new DynamoDBTestContainer())
    {
    }

    [Fact]
    public override async Task DeleteAll()
    {
        // Arrange
        int itemsCount = 5;
        List<TestDynamoDbItem> list = new();

        for (var i = 0; i < itemsCount; i++)
        {
            list.Add(CreateNewItem());
        }

        await Collection.InsertAsync(list);

        // Act
        var deletedItems = await Collection.DeleteCollectionAsync();

        // Assert
        deletedItems.Should().BeTrue();
    }

    [Fact]
    public override async Task DeleteItemById_WhenItemDoesntExists()
    {
        // Arrange
        var item = CreateNewItem();

        // Act
        var deleted = () => Collection.DeleteAsync(item.Id);

        // Assert
        deleted.Should().ThrowExactlyAsync<DatabaseException>();
    }
}