using FluentAssertions;
using ManagedCode.Database.Core.Exceptions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using System.Threading.Tasks;

namespace ManagedCode.Database.Tests.CosmosTests;

public class CosmosCollectionTests : BaseCollectionTests<string, TestCosmosItem>
{
    public CosmosCollectionTests() : base(new CosmosTestContainer())
    {
    }

    public override async Task DeleteItemById_WhenItemDoesntExists()
    {
        var baseMethod = async () => await base.DeleteItemById_WhenItemDoesntExists();

        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
    }
}