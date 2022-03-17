using ManagedCode.Repository.EntityFramework.Interfaces;

namespace ManagedCode.Repository.EntityFramework.PostgreSQL;

public class PostgresItem<TId> : IEFItem<TId>
{
    public TId Id { get; set; }
}