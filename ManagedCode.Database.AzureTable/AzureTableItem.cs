using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable;

public class AzureTableItem : TableEntity, IItem<TableId>
{
    private TableId _id;
    public AzureTableItem()
    {
        _id = new TableId(this);
    }
    
    [IgnoreProperty]
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