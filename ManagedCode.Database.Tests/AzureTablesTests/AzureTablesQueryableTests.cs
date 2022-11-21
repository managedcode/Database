using ManagedCode.Database.AzureTables;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.AzureTablesTests;

public class AzureTablesQueryableTests : BaseQueryableTests<TableId, TestAzureTablesItem>
{
    public AzureTablesQueryableTests() : base(new AzureTablesTestContainer())
    {
    }
}