using ManagedCode.Database.Core;

namespace ManagedCode.Database.SQLite;

public interface ISQLiteRepository<TItem> : IRepository<int, TItem> where TItem : SQLiteItem, new()
{
}