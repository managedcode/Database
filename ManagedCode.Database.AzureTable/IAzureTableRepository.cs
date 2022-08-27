using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable;

public interface IAzureDBCollection<T> : IDBCollection<TableId, T>
    where T : class, IItem<TableId>, ITableEntity, new()
{
}