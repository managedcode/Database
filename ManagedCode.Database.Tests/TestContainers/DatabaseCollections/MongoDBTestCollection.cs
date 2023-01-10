using Xunit;

namespace ManagedCode.Database.Tests.TestContainers.DatabaseCollections
{
    [CollectionDefinition("MongoDB collection")]
    public class MongoDBTestCollection : ICollectionFixture<MongoDBTestContainer>
    {
    }
}
