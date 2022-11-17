using ManagedCode.Database.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ManagedCode.Database.MongoDB;

public class MongoDBItem : IItem<ObjectId>
{
    public MongoDBItem()
    {
        Id = ObjectId.GenerateNewId();
    }

    public MongoDBItem(string id)
    {
        Id = ObjectId.Parse(id);
    }

    public MongoDBItem(ObjectId id)
    {
        Id = id;
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
}