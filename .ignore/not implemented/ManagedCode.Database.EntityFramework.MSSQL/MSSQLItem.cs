using ManagedCode.Database.EntityFramework.Interfaces;

namespace ManagedCode.Database.EntityFramework.MSSQL;

public class MSSQLItem<TId> : IEFItem<TId>
{
    public TId Id { get; set; }
}