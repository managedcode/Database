using Xunit;

namespace ManagedCode.Database.Tests.TestContainers.DatabaseCollections
{
    [CollectionDefinition("DynamoDB collection")]
    public class DynamoDBTestCollection : ICollectionFixture<DynamoDBTestContainer>
    {
    }
}
