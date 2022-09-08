using ManagedCode.Database.EntityFramework.Interfaces;

namespace ManagedCode.Database.EntityFramework.MSSQL;

public interface IMSSQLRepository<TId, TItem> : IEFRepository<TId, TItem, MSSQLDatabaseContext>
    where TItem : IEFItem<TId>, new()
{
}