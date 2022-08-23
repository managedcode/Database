using ManagedCode.Database.Core;
using MongoDB.Bson;

namespace ManagedCode.Repository.MongoDB;

public interface IRealmRepository<TItem> : IRepository<ObjectId, TItem> where TItem : class, IItem<ObjectId>
{
}