using System;
using System.Threading.Tasks;
using FluentAssertions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using MongoDB.Bson;

namespace ManagedCode.Database.Tests.MongoDBTests;

public class MongoDBQueryableTests : BaseQueryableTests<ObjectId, TestMongoDBItem>
{
    public MongoDBQueryableTests() : base(new MongoDBTestContainer())
    {
    }

    public override async Task OrderBy_TakeNull_ReturnException()
    {
        // Arrange
        int itemsCountToInsert = 1;

        var guid = Guid.NewGuid().ToString();

        await CreateAndInsertItemsAsync(itemsCountToInsert);

        // Act
        var itemsResult = () => Collection.Query
            .OrderBy(null)
            .ToListAsync();

        // Assert
        await itemsResult
            .Should()
            .ThrowAsync<ArgumentNullException>();   
    }
}