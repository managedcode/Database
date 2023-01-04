using FluentAssertions;
using ManagedCode.Database.Core.Exceptions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ManagedCode.Database.Tests.CosmosTests;

public class CosmosCollectionTests : BaseCollectionTests<string, TestCosmosItem>
{
    public CosmosCollectionTests(ITestOutputHelper testOutputHelper) : base(new CosmosTestContainer(testOutputHelper))
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