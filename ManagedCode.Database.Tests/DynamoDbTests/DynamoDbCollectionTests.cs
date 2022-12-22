using Amazon.DynamoDBv2.DocumentModel;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.DynamoDbTests;

public class DynamoDbCollectionTests : BaseCollectionTests<string, TestDynamoDbItem>
{
    public DynamoDbCollectionTests() : base(new DynamoDBTestContainer())
    {
    }
}