using System;

namespace ManagedCode.Database.Tests.Common
{
    public class InMemoryItem : IBaseItem<int>
    {
        public int Id { get; set; }
        public string StringData { get; set; }
        public int IntData { get; set; }
        public long LongData { get; set; }
        public float FloatData { get; set; }
        public double DoubleData { get; set; }
        public DateTime DateTimeData { get; set; }
    }
}