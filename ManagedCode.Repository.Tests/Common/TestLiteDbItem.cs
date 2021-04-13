using System;
using ManagedCode.Repository.LiteDB;

namespace ManagedCode.Repository.Tests.Common
{
    public class TestLiteDbItem : LiteDbItem<string>
    {
        public TestLiteDbItem()
        {
            Id = Guid.NewGuid().ToString();
        }

        public TestLiteDbItem(string id)
        {
            Id = id;
        }

        public string PartKey { get; set; }
        public string Data { get; set; }
        public string RowKey { get; set; }
        public int IntData { get; set; }
    }
}