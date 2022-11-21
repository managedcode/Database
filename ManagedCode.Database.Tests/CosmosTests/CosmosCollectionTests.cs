using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.CosmosTests;

public class CosmosCollectionTests : BaseCollectionTests<string, TestCosmosItem>
{
    public CosmosCollectionTests() : base(new CosmosTestContainer())
    {
    }
}