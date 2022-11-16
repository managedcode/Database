using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using ManagedCode.Database.Core;
using ManagedCode.Database.MongoDB;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using MongoDB.Bson;
using Xunit;

namespace ManagedCode.Database.Tests.MongoDbTests
{
    public class MongoDbQueryableTests : BaseQueryableTests<ObjectId, TestMongoDbItem>, IAsyncLifetime
    {
        private readonly MongoDBDatabase _dbDatabase;
        private readonly TestcontainersContainer _mongoDBContainer;

        public MongoDbQueryableTests()
        {
            _dbDatabase = new MongoDBDatabase(new MongoDBOptions()
            {
                ConnectionString = "mongodb://localhost:27017",
                DataBaseName = "db"
            });

            _mongoDBContainer = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("mongo")
                .WithPortBinding(27017, 27017)
                .Build();
        }

        protected override IDatabaseCollection<ObjectId, TestMongoDbItem> Collection =>
            _dbDatabase.GetCollection<TestMongoDbItem>();

        protected override ObjectId GenerateId()
        {
            return ObjectId.GenerateNewId();
        }


        public override async Task InitializeAsync()
        {
            await _mongoDBContainer.StartAsync();
            await _dbDatabase.InitializeAsync();
        }

        public override async Task DisposeAsync()
        {
            await _dbDatabase.DisposeAsync();
            await _mongoDBContainer.StopAsync();
        }
    }
}