using ManagedCode.Repository.Core;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Realms;

namespace ManagedCode.Repository.MongoDB;

public class RealmDbItem : RealmObject, IItem<ObjectId>
{
    public RealmDbItem()
    {
        Id = ObjectId.GenerateNewId();
    }

    public RealmDbItem(string id)
    {
        Id = ObjectId.Parse(id);
    }

    public RealmDbItem(ObjectId id)
    {
        Id = id;
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }
}