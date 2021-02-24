using ManagedCode.Repository.CosmosDB;

namespace ManagedCode.Repository.Tests.Common
{
    public class CosmosDbItem : CosmosDB.CosmosDbItem
    {
        public CosmosDbItem()
        {
        }

        public CosmosDbItem(string id) : base(id)
        {
        }

        public CosmosDbItem(string id, string partKey) : base(id)
        {
            PartKey = partKey;
        }

        public string PartKey { get; set; }
        public string Data { get; set; }
        public string RowKey { get; set; }
        public int IntData { get; set; }
    }
}