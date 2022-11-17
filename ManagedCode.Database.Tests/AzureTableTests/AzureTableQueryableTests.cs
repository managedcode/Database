using System.Threading.Tasks;
using ManagedCode.Database.AzureTable;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using Xunit;

namespace ManagedCode.Database.Tests.AzureTableTests;

public class AzureTableQueryableTests : BaseQueryableTests<TableId, TestAzureTableItem>, IAsyncLifetime
{
    private readonly AzureTableTestContainer _azureTableContainer;

    public AzureTableQueryableTests()
    {
        _azureTableContainer = new AzureTableTestContainer();
    }

    protected override IDatabaseCollection<TableId, TestAzureTableItem> Collection =>
        _azureTableContainer.GetCollection();

    protected override TableId GenerateId()
    {
        return _azureTableContainer.GenerateId();
    }

    public override async Task DisposeAsync()
    {
        await _azureTableContainer.DisposeAsync();
    }

    public override async Task InitializeAsync()
    {
        await _azureTableContainer.InitializeAsync();
    }
}