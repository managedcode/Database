using Azure.Data.Tables;
using ManagedCode.Database.Core;

namespace ManagedCode.Database.AzureTables;

public interface IAzuresDatabaseCollection<T> : IDatabaseCollection<TableId, T>
    where T : class, IItem<TableId>, ITableEntity, new()
{
}