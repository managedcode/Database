using ManagedCode.Repository.Core;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Repository.AzureTable
{
    public class AzureTableRepositoryItem : TableEntity, IRepositoryItem<AzureTableId>
    {
        public AzureTableRepositoryItem()
        {
            Id = new AzureTableId(this);
        }

        [IgnoreProperty] public AzureTableId Id { get; set; }
    }
}