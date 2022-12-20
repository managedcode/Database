using ManagedCode.Database.AzureTables;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.AzureTablesTests
{
    public class AzureTablesMultiThreadingTests : BaseMultiThreadingTests<TableId, TestAzureTablesItem>
    {
        public AzureTablesMultiThreadingTests() 
            : base(new AzureTablesTestContainer())
        {
        }
    }
}
