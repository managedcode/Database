using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.ZoneTreeTests;

public class ZoneTreeCollectionTests : BaseCollectionTests<string, TestZoneTreeItem>
{
    public ZoneTreeCollectionTests() : base(new ZoneTreeTestContainer())
    {
    }
}