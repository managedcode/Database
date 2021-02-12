using System;
using ManagedCode.Repository.CosmosDB;

namespace ManagedCode.Repository.Tests.Common
{
    public class LiteDbItem : BaseLiteDbItem<string>
    {
        public LiteDbItem()
        {
            Id = Guid.NewGuid().ToString();
        }

        public LiteDbItem(string id)
        {
            Id = id;
        }

        public string PartKey { get; set; }
        public string Data { get; set; }
        public string RowKey { get; set; }
        public int IntData { get; set; }
    }
}