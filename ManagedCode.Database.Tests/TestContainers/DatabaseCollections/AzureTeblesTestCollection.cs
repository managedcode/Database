using Xunit;

namespace ManagedCode.Database.Tests.TestContainers.DatabaseCollections
{
    [CollectionDefinition("AzureTables collection")]
    public class AzureTeblesTestCollection : ICollectionFixture<AzureTablesTestContainer>
    {
    }
}
