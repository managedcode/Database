using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.ZoneTreeTests;

public class ZoneTreeQueryableTests : BaseQueryableTests<string, TestZoneTreeItem>
{
    public ZoneTreeQueryableTests() : base(new ZoneTreeTestContainer())
    {
    }
}