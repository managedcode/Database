using ManagedCode.Repository.EntityFramework.Interfaces;

namespace ManagedCode.Repository.EntityFramework.PostgreSQL;

public interface IPostgresRepository<TId, TItem> : IEFRepository<TId, TItem, PostgresDatabaseContext>
    where TItem : IEFItem<TId>, new()
{
}