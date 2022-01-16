using ManagedCode.Repository.EntityFramework.Interfaces;

namespace ManagedCode.Repository.EntityFramework.MSSQL
{
    public interface IMSSQLRepository<TId, TItem> : IEFRepository<TId, TItem, MSSQLDatabaseContext>
        where TItem : IEFItem<TId>, new()
    {
    }
}
