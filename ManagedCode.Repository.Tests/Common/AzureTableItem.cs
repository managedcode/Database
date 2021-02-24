using ManagedCode.Repository.AzureTable;

namespace ManagedCode.Repository.Tests.Common
{
    public class AzureTableItem : AzureTable.AzureTableItem
    {
        public string Data { get; set; }
        public int IntData { get; set; }
    }
}