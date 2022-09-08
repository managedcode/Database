using ManagedCode.Database.EntityFramework.Interfaces;

namespace ManagedCode.Database.EntityFramework.PostgreSQL;

public interface IPostgresRepository<TId, TItem> : IEFRepository<TId, TItem, PostgresDatabaseContext>
    where TItem : IEFItem<TId>, new()
{
}