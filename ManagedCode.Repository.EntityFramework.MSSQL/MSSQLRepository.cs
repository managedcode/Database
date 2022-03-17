namespace ManagedCode.Repository.EntityFramework.MSSQL;

public class MSSQLRepository<TId, TItem> : EFRepository<TId, TItem, MSSQLDatabaseContext>, IMSSQLRepository<TId, TItem>
    where TItem : MSSQLItem<TId>, new()
{
    public MSSQLRepository(MSSQLDatabaseContext context) : base(context)
    {
    }
}