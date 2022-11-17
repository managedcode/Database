using System.Threading.Tasks;
using ManagedCode.Database.AzureTables;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using Xunit;

namespace ManagedCode.Database.Tests.AzureTablesTests;

public class AzureTablesQueryableTests : BaseQueryableTests<TableId, TestAzureTablesItem>, IAsyncLifetime
{
    private readonly AzureTablesTestContainer _azureTablesContainer;

    public AzureTablesQueryableTests()
    {
        _azureTablesContainer = new AzureTablesTestContainer();
    }

    protected override IDatabaseCollection<TableId, TestAzureTablesItem> Collection =>
        _azureTablesContainer.GetCollection();

    protected override TableId GenerateId()
    {
        return _azureTablesContainer.GenerateId();
    }

    public override async Task DisposeAsync()
    {
        await _azureTablesContainer.DisposeAsync();
    }

    public override async Task InitializeAsync()
    {
        await _azureTablesContainer.InitializeAsync();
    }
}