using Xunit;

namespace ManagedCode.Database.Tests.TestContainers.DatabaseCollections
{
    [CollectionDefinition("MongoDBTestCollection")]
    public class MongoDBTestCollection : ICollectionFixture<MongoDBTestContainer>
    {
    }
}
