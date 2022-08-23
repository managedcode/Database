namespace ManagedCode.Database.EntityFramework.PostgreSQL;

public class PostgresRepository<TId, TItem> : EFRepository<TId, TItem, PostgresDatabaseContext>, IPostgresRepository<TId, TItem>
    where TItem : PostgresItem<TId>, new()
{
    public PostgresRepository(PostgresDatabaseContext context) : base(context)
    {
    }
}