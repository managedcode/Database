using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using MongoDB.Bson;

namespace ManagedCode.Database.Tests.MongoDBTests;

public class MongoDBDatabaseTests : BaseDatabaseTests<ObjectId, TestMongoDBItem>
{
    public MongoDBDatabaseTests() : base(new MongoDBTestContainer())
    {
    }
}