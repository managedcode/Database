using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.SQLiteTests;

public class SQLiteRepositoryTests : BaseQueryableTests<int, TestSQLiteItem>
{
    public SQLiteRepositoryTests() : base(new SQLiteTestContainer())
    {
    }
}