using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using MongoDB.Bson;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Database.Tests.MongoDBTests;

public class MongoDBCollectionTests : BaseCollectionTests<ObjectId, TestMongoDBItem>, IClassFixture<MongoDBTestContainer>
{
    public MongoDBCollectionTests(MongoDBTestContainer container) : base(container)
    {
    }
}