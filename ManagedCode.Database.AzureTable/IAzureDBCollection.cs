using Azure.Data.Tables;
using ManagedCode.Database.Core;

namespace ManagedCode.Database.AzureTable;

public interface IAzureDBCollection<T> : IDBCollection<TableId, T>
    where T : class, IItem<TableId>, ITableEntity, new()
{
}