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
        private readonly MongoDbDatabase _database;
        private readonly TestcontainersContainer _mongoDBContainer;

        public MongoDbQueryableTests()
        {
            _database = new MongoDbDatabase(new MongoDbRepositoryOptions()
            {
                ConnectionString = "mongodb://localhost:27017",
                DataBaseName = "db"
            });

            _mongoDBContainer = new TestcontainersBuilder<TestcontainersContainer>()
                .WithImage("mongo")
                .WithPortBinding(27017, 27017)
                .Build();
        }

        protected override IDBCollection<ObjectId, TestMongoDbItem> Collection =>
            _database.GetCollection<ObjectId, TestMongoDbItem>();

        protected override ObjectId GenerateId()
        {
            return ObjectId.GenerateNewId();
        }


        public override async Task InitializeAsync()
        {
            await _mongoDBContainer.StartAsync();
            await _database.InitializeAsync();
        }

        public override async Task DisposeAsync()
        {
            await _database.DisposeAsync();
            await _mongoDBContainer.StopAsync();
        }
    }
}