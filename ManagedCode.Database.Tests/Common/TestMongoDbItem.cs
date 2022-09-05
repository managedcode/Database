using System;
using ManagedCode.Database.MongoDB;
using MongoDB.Bson;

namespace ManagedCode.Database.Tests.Common;

public class TestMongoDbItem : MongoDbItem, IBaseItem<ObjectId>
{
    public string StringData { get; set; }
    public int IntData { get; set; }
    public float FloatData { get; set; }
    public DateTime DateTimeData { get; set; }
}