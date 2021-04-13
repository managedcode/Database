using ManagedCode.Repository.Core;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace ManagedCode.Repository.MongoDB
{
    public class MongoDbItem : IItem<ObjectId>
    {
        public MongoDbItem()
        {
            Id = ObjectId.GenerateNewId();
        }

        public MongoDbItem(string id)
        {
            Id = ObjectId.Parse(id);
        }

        public MongoDbItem(ObjectId id)
        {
            Id = id;
        }

        public ObjectId Id { get; set; }
    }
}