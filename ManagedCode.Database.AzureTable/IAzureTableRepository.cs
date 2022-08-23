using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable;

public interface IAzureTableRepository<T> : IRepository<TableId, T>
    where T : class, IItem<TableId>, ITableEntity, new()
{
}