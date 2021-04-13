using MongoDB.Bson;

namespace ManagedCode.Repository.Tests.Common
{
    public class TestMongoDbItem : ManagedCode.Repository.MongoDB.MongoDbItem
    {
        public TestMongoDbItem() : base()
        {
        }

        public TestMongoDbItem(string id) : base(id)
        {
        }

        public TestMongoDbItem(string id, string partKey) : base(id)
        {
            Id = ObjectId.Parse(id);
            PartKey = partKey;
        }

        public string PartKey { get; set; }
        public string Data { get; set; }
        public string RowKey { get; set; }
        public int IntData { get; set; }
    }
}