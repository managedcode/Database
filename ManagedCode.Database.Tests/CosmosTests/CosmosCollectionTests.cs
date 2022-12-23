using FluentAssertions;
using ManagedCode.Database.Core.Exceptions;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using System.Threading.Tasks;

namespace ManagedCode.Database.Tests.CosmosTests;

static class TET
{
    public static CosmosTestContainer CONT = new CosmosTestContainer();
}

public class CosmosCollectionTests : BaseCollectionTests<string, TestCosmosItem>
{
    public CosmosCollectionTests() : base(TET.CONT)
    {
    }

    public override async Task DeleteItemById_WhenItemDoesntExists()
    {
        var baseMethod = async () => await base.DeleteItemById_WhenItemDoesntExists();

        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
    }

    public override async Task DeleteListOfItemsById_WhenItemsDontExist()
    {
        var baseMethod = async () => await base.DeleteListOfItemsById_WhenItemsDontExist();

        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
    }

    public override async Task DeleteListOfItems_WhenItemsDontExist()
    {
        var baseMethod = async () => await base.DeleteListOfItems_WhenItemsDontExist();

        await baseMethod.Should().ThrowExactlyAsync<DatabaseException>();
    }
}