using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Repository.AzureTable
{
    public class AzureTableId
    {
        private readonly ITableEntity _entity;

        public AzureTableId(ITableEntity entity)
        {
            _entity = entity;
        }

        public AzureTableId(string partitionKey, string rowKey)
        {
            _entity = new TableEntity(partitionKey, rowKey);
        }

        public string RowKey
        {
            get => _entity.RowKey;
            set => _entity.RowKey = value;
        }

        public string PartitionKey
        {
            get => _entity.PartitionKey;
            set => _entity.PartitionKey = value;
        }
    }
}