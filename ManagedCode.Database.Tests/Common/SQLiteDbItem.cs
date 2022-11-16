using System;
using SQLite;

namespace ManagedCode.Database.Tests.Common
{
    public class SQLiteDbItem : IBaseItem<int>
    {
        [PrimaryKey]
        public int Id { get; set; }

        public string StringData { get; set; }
        public int IntData { get; set; }
        public long LongData { get; set; }
        public float FloatData { get; set; }
        public double DoubleData { get; set; }
        public DateTime DateTimeData { get; set; }
    }
}