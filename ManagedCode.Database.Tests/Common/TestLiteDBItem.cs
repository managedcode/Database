using System;
using ManagedCode.Database.LiteDB;

namespace ManagedCode.Database.Tests.Common;

public class TestLiteDBItem : LiteDBItem<string>, IBaseItem<string>
{
    public TestLiteDBItem()
    {
        Id = Guid.NewGuid().ToString();
    }

    public string StringData { get; set; }
    public int IntData { get; set; }
    public long LongData { get; set; }
    public float FloatData { get; set; }
    public double DoubleData { get; set; }
    public DateTime DateTimeData { get; set; }
}