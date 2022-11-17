using System;
using System.IO;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.SQLite;
using ManagedCode.Database.Tests.BaseTests;
using ManagedCode.Database.Tests.Common;

namespace ManagedCode.Database.Tests.SQLiteTests;

public class SQLiteCollectionTests : BaseCollectionTests<int, SQLiteDbItem>
{
    private static int _count;
    private readonly SQLiteDatabase _database;


    public SQLiteCollectionTests()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), "sqlite_test.db");

        _database = new SQLiteDatabase(new SQLiteRepositoryOptions
        {
            ConnectionString = dbPath,
        });
    }

    public override async Task InitializeAsync()
    {
        await _database.InitializeAsync();
    }

    public override async Task DisposeAsync()
    {
        await _database.DeleteAsync();
    }

    protected override IDatabaseCollection<int, SQLiteDbItem> Collection =>
        _database.GetCollection<int, SQLiteDbItem>();

    private static string GetTempDbName()
    {
        return Path.Combine(Environment.CurrentDirectory, Guid.NewGuid() + "sqlite_test.db");
    }

    protected override int GenerateId()
    {
        _count++;
        return _count;
    }
}