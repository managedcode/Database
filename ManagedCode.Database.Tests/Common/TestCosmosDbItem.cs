using System;
using ManagedCode.Database.CosmosDB;

namespace ManagedCode.Database.Tests.Common;

public class TestCosmosDbItem : CosmosDbItem, IBaseItem<string>
{
    public string StringData { get; set; }
    public int IntData { get; set; }
    public float FloatData { get; set; }
    public DateTime DateTimeData { get; set; }
}