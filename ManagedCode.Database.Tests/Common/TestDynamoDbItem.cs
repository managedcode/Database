using System;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using ManagedCode.Database.DynamoDB;

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
    public DateTime DateTimeData { get; set; }
}