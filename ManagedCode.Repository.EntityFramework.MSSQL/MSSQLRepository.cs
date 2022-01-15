namespace ManagedCode.Repository.EntityFramework.MSSQL
{
    public class MSSQLRepository<TId, TItem> : EFRepository<TId, TItem, MSSQLDatabaseContext>
        where TItem : class, IEFItem<TId>, new()
    {
        public MSSQLRepository(MSSQLDatabaseContext context) : base(context)
        {
        }
    }
}
