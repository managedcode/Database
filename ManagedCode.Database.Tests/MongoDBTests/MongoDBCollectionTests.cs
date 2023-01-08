using FluentAssertions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using MongoDB.Bson;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Database.Tests.MongoDBTests;

public class MongoDBCollectionTests : BaseCollectionTests<ObjectId, TestMongoDBItem>
{
    public MongoDBCollectionTests(ITestOutputHelper testOutputHelper) : base(new MongoDBTestContainer(testOutputHelper))
    {
    }
}