using ManagedCode.Database.Core;

namespace ManagedCode.Database.LiteDB;

public interface ILiteDbRepository<TId, TItem> : IRepository<TId, TItem> where TItem : LiteDbItem<TId>, IItem<TId>, new()
{
}