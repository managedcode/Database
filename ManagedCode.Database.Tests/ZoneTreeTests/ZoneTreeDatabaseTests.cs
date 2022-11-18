using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.ZoneTreeTests;

public class ZoneTreeDatabaseTests : BaseDatabaseTests<string, TestZoneTreeItem>
{
    public ZoneTreeDatabaseTests() : base(new ZoneTreeTestContainer())
    {
    }
}