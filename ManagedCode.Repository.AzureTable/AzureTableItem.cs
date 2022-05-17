using ManagedCode.Repository.Core;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Repository.AzureTable;

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