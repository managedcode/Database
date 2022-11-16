using Azure.Data.Tables;
using ManagedCode.Database.Core;

namespace ManagedCode.Database.AzureTable
{
    public interface IAzureDatabaseCollection<T> : IDatabaseCollection<TableId, T>
        where T : class, IItem<TableId>, ITableEntity, new()
    {
    }
}