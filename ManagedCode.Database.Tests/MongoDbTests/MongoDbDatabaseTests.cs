using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.Core;
using ManagedCode.Database.MongoDB;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using MongoDB.Bson;
using Xunit;

namespace ManagedCode.Database.Tests.MongoDbTests
{
    public class MongoDbDatabaseTests : BaseDatabaseTests<ObjectId, TestMongoDbItem>
    {
        public MongoDbDatabaseTests() : base(new MongoDBTestContainer())
        {
        }
    }
}