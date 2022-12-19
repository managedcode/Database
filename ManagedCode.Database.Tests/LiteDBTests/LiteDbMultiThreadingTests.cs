using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.LiteDBTests
{
    public class LiteDBMultiThreadingTests : BaseMultiThreadingTests<string, TestLiteDBItem>
    {
        public LiteDBMultiThreadingTests() : base(new LiteDBTestContainer())
        {
        }
    }
}
