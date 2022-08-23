using ManagedCode.Database.MongoDB;
using ManagedCode.Database.Tests.Common;
using MongoDB.Bson;

namespace ManagedCode.Database.Tests
{
    
    public class MongoDbRepositoryTests : BaseRepositoryTests<ObjectId, TestMongoDbItem>
    {
        public const string ConnectionString =
            "mongodb://localhost:27017";

        public MongoDbRepositoryTests() : base(new MongoDbRepository<TestMongoDbItem>(new MongoDbRepositoryOptions
        {
            ConnectionString = ConnectionString,
            DataBaseName = "db"
        }))
        {
            Repository.InitializeAsync().Wait();
        }

        protected override ObjectId GenerateId()
        {
            return ObjectId.GenerateNewId();
        }
    }
}

