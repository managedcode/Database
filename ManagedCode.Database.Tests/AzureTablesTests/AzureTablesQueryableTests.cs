using System.Threading.Tasks;
using ManagedCode.Database.AzureTables;
using ManagedCode.Database.Core;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;
using Xunit;

namespace ManagedCode.Database.Tests.AzureTablesTests;

public class AzureTablesQueryableTests : BaseQueryableTests<TableId, TestAzureTablesItem>
{
    public AzureTablesQueryableTests() : base(new AzureTablesTestContainer())
    {
    }
}