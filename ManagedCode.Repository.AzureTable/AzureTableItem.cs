using ManagedCode.Repository.Core;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Repository.AzureTable
{
    public class AzureTableItem : TableEntity, IItem<TableId>
    {
        public AzureTableItem()
        {
            Id = new TableId(this);
        }

        [IgnoreProperty]
        public TableId Id { get; set; }
    }
}