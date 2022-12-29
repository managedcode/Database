using System;
using ManagedCode.Database.DynamoDB;
using Newtonsoft.Json;

namespace ManagedCode.Database.Tests.Common;

public class TestDynamoDbItem : DynamoDBItem<string>, IBaseItem<string>
{
    public TestDynamoDbItem()
    {
        Id = Guid.NewGuid().ToString();
    }

    public string StringData { get; set; }
    public int IntData { get; set; }
    public long LongData { get; set; }
    public float FloatData { get; set; }
    public double DoubleData { get; set; }

    [JsonIgnore]
    public DateTime DateTimeData { get; set; }

    [JsonProperty("date")]
    public string CustomDate
    {
        get { return DateTimeData.ToString(); }
        set { DateTimeData = DateTime.Parse(value); }
    }
}