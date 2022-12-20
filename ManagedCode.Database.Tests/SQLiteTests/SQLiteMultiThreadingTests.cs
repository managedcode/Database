using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.SQLiteTests
{
    public class SQLiteMultiThreadingTests : BaseMultiThreadingTests<int, TestSQLiteItem>
    {
        public SQLiteMultiThreadingTests()
            : base(new SQLiteTestContainer())
        {
        }
    }
}
