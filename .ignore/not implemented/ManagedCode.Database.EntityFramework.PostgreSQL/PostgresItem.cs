using ManagedCode.Database.EntityFramework.Interfaces;

namespace ManagedCode.Database.EntityFramework.PostgreSQL;

public class PostgresItem<TId> : IEFItem<TId>
{
    public TId Id { get; set; }
}