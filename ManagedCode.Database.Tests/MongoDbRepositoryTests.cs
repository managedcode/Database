using ManagedCode.Database.Core;
using ManagedCode.Database.MongoDB;
using ManagedCode.Database.Tests.Common;
using MongoDB.Bson;

namespace ManagedCode.Database.Tests
{
    
    public class MongoDbRepositoryTests : BaseRepositoryTests<ObjectId, TestMongoDbItem>
    {
        public const string ConnectionString =
            "mongodb://localhost:27017";
        
        private MongoDbDatabase _databaseb;

        public MongoDbRepositoryTests()
        {

            _databaseb = new MongoDbDatabase(new MongoDbRepositoryOptions()
            {
                ConnectionString = ConnectionString,
                DataBaseName = "db"
            });
            
        }

        protected override IDBCollection<ObjectId, TestMongoDbItem> Collection => _databaseb.GetCollection<ObjectId, TestMongoDbItem>();

        protected override ObjectId GenerateId()
        {
            return ObjectId.GenerateNewId();
        }
    }
}

