using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Repository.AzureTable
{
    public class TableId
    {
        private readonly ITableEntity _entity;

        public TableId(ITableEntity entity)
        {
            _entity = entity;
        }

        public TableId(string partitionKey, string rowKey)
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