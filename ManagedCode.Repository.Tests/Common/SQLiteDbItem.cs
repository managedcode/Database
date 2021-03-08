using ManagedCode.Repository.Core;
using SQLite;

namespace ManagedCode.Repository.Tests.Common
{
    public class SQLiteDbItem : IItem<int>
    {
        public string PartKey { get; set; }
        public string Data { get; set; }
        public string RowKey { get; set; }
        public int IntData { get; set; }

        [PrimaryKey]
        public int Id { get; set; }
    }
}