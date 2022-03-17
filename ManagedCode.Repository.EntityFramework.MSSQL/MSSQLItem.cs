using ManagedCode.Repository.EntityFramework.Interfaces;

namespace ManagedCode.Repository.EntityFramework.MSSQL;

public class MSSQLItem<TId> : IEFItem<TId>
{
    public TId Id { get; set; }
}