using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.SQLiteTests;

public class SQLiteDatabaseTests : BaseDatabaseTests<int, TestSQLiteItem>
{
    public SQLiteDatabaseTests() : base(new SQLiteTestContainer())
    {
    }
}