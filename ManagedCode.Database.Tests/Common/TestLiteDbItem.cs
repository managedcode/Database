using System;
using ManagedCode.Database.EntityFramework.PostgreSQL;
using ManagedCode.Database.LiteDB;

namespace ManagedCode.Database.Tests.Common;

public class TestLiteDbItem : LiteDbItem<string>, IBaseItem<string>
{
    public TestLiteDbItem()
    {
        Id = Guid.NewGuid().ToString();
    }

    public string StringData { get; set; }
    public int IntData { get; set; }
    public float FloatData { get; set; }
    public DateTime DateTimeData { get; set; }
}


public class TestPostgresItem : PostgresItem<int>, IBaseItem<int>
{
    public string StringData { get; set; }
    public int IntData { get; set; }
    public float FloatData { get; set; }
    public DateTime DateTimeData { get; set; }
}


