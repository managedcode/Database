using System;
using System.Runtime.Serialization;
using Azure;
using Azure.Data.Tables;
using ManagedCode.Database.Core;

namespace ManagedCode.Database.AzureTable;

public class AzureTableItem : ITableEntity, IItem<TableId>
{
    private TableId _id;

    public AzureTableItem()
    {
        _id = new TableId(this);
    }

    public string PartitionKey { get; set; } = default!;
    public string RowKey { get; set; } = default!;
    public DateTimeOffset? Timestamp { get; set; } = default!;
    public ETag ETag { get; set; }

    [IgnoreDataMember]
    public TableId Id
    {
        get => _id;
        set
        {
            _id = value;
            _id.SetEntity(this);
        }
    }
}