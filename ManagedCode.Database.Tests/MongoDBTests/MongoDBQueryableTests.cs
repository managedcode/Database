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
}