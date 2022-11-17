using System;
using System.IO;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.SQLite;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;
using ManagedCode.Database.Tests.TestContainers;

namespace ManagedCode.Database.Tests.SQLiteTests;

public class SQLiteCollectionTests : BaseCollectionTests<int, SQLiteDbItem>
{
    public SQLiteCollectionTests() : base(new SQLiteTestContainer())
    {
    }
}