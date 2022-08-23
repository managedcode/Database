using ManagedCode.Database.Core;

namespace ManagedCode.Database.CosmosDB;

public interface ICosmosDbRepository<TItem> : IRepository<string, TItem> where TItem : CosmosDbItem, IItem<string>, new()
{
}