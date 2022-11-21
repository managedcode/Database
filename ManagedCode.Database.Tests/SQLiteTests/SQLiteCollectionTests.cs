using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.SQLiteTests;

public class SQLiteCollectionTests : BaseCollectionTests<int, TestSQLiteItem>
{
    public SQLiteCollectionTests() : base(new SQLiteTestContainer())
    {
    }
}