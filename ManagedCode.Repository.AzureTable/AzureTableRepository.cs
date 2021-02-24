using ManagedCode.Repository.Core;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Repository.AzureTable
{
    public class AzureTableRepository<TItem> : BaseAzureTableRepository<TableId, TItem>, IAzureTableRepository<TItem> 
        where TItem : class, IItem<TableId>, ITableEntity, new()
    {
        public AzureTableRepository(string connectionString) : base(connectionString)
        {
        }

        public AzureTableRepository(StorageCredentials tableStorageCredentials, StorageUri tableStorageUri) : base(tableStorageCredentials, tableStorageUri)
        {
        }
    }
}