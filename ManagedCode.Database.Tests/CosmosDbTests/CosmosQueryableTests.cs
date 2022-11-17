using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.CosmosDbTests;

public class CosmosQueryableTests : BaseQueryableTests<string, TestCosmosItem>
{
    public CosmosQueryableTests() : base(new CosmosTestContainer())
    {
    }
}