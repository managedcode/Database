using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.ZoneTreeTests
{
    public class ZoneTreeMultiThreadingTests : BaseMultiThreadingTests<string, TestZoneTreeItem>
    {
        public ZoneTreeMultiThreadingTests() : base(new ZoneTreeTestContainer())
        {
        }
    }
}
