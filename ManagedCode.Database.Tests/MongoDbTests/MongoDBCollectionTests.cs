using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.Core;
using ManagedCode.Database.MongoDB;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using MongoDB.Bson;

namespace ManagedCode.Database.Tests.MongoDbTests;

public class MongoDBCollectionTests : BaseCollectionTests<ObjectId, TestMongoDbItem>
{
    public MongoDBCollectionTests() : base(new MongoDBTestContainer())
    {
    }
}