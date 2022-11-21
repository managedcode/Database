using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using MongoDB.Bson;

namespace ManagedCode.Database.Tests.MongoDBTests;

public class MongoDBCollectionTests : BaseCollectionTests<ObjectId, TestMongoDBItem>
{
    public MongoDBCollectionTests() : base(new MongoDBTestContainer())
    {
    }
}