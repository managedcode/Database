using Azure.Data.Tables;

namespace ManagedCode.Database.AzureTable;

public class TableId
{
    private ITableEntity _internalEntity;

    public TableId(ITableEntity entity)
    {
        _internalEntity = entity;
    }

    public TableId(string partitionKey, string rowKey)
    {
        _internalEntity = new TableEntity(partitionKey, rowKey);
    }

    public string PartitionKey
    {
        get => _internalEntity.PartitionKey;
        set => _internalEntity.PartitionKey = value;
    }

    public string RowKey
    {
        get => _internalEntity.RowKey;
        set => _internalEntity.RowKey = value;
    }

    public void SetEntity(ITableEntity entity)
    {
        entity.PartitionKey = PartitionKey;
        entity.RowKey = RowKey;
        _internalEntity = entity;
    }

    public override string ToString()
    {
        return $"PartitionKey:{PartitionKey};RowKey:{PartitionKey};";
    }

    public override bool Equals(object? obj)
    {
        if (obj is TableId tableId) return tableId.PartitionKey == PartitionKey && tableId.RowKey == RowKey;

        return false;
    }

    public override int GetHashCode()
    {
        return PartitionKey.GetHashCode() ^ RowKey.GetHashCode();
    }
}