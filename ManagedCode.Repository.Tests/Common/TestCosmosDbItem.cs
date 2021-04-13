using ManagedCode.Repository.MongoDB;

namespace ManagedCode.Repository.Tests.Common
{
    public class TestCosmosDbItem : CosmosDB.CosmosDbItem
    {
        public TestCosmosDbItem()
        {
        }

        public TestCosmosDbItem(string id) : base(id)
        {
        }

        public TestCosmosDbItem(string id, string partKey) : base(id)
        {
            PartKey = partKey;
        }

        public string PartKey { get; set; }
        public string Data { get; set; }
        public string RowKey { get; set; }
        public int IntData { get; set; }
    }
}