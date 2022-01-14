using ManagedCode.Repository.Core;

namespace ManagedCode.Repository.MSSQL
{
    public class MSSQLItem<TId> : IItem<TId>
    {
        public TId Id { get; set; }
    }
}
