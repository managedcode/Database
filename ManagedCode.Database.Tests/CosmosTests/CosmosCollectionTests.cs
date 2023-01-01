using FluentAssertions;
using ManagedCode.Database.Core.Exceptions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using System.Threading.Tasks;
using Xunit;

namespace ManagedCode.Database.Tests.CosmosTests;

[Collection("cosmos")]
public class CosmosCollectionTests : BaseCollectionTests<string, TestCosmosItem>, IClassFixture<CosmosDockerContainer>
{
    public CosmosCollectionTests(CosmosDockerContainer container) : base(new CosmosTestContainer(container))
    {
    }

    public override async Task DeleteItemById_WhenItemDoesntExists()
    {
        var baseMethod = () => base.DeleteItemById_WhenItemDoesntExists();

        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
    }

    public override async Task DeleteListOfItemsById_WhenItemsDontExist()
    {
        var baseMethod = () => base.DeleteListOfItemsById_WhenItemsDontExist();

        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
    }

    public override async Task DeleteListOfItems_WhenItemsDontExist()
    {
        var baseMethod = () => base.DeleteListOfItems_WhenItemsDontExist();

        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
    }
}