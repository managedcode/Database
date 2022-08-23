using ManagedCode.Database.Core;
using MongoDB.Bson;

namespace ManagedCode.Database.MongoDB;

public interface IMongoDbRepository<TItem> : IRepository<ObjectId, TItem> where TItem : class, IItem<ObjectId>
{
}