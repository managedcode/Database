using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.SQLite;
using ManagedCode.Database.Tests.Common;

namespace ManagedCode.Database.Tests.TestContainers;

public class SQLiteTestContainer : ITestContainer<int, SQLiteDbItem>
{
    private static volatile int _count;
    private readonly SQLiteDatabase _database;

    public SQLiteTestContainer()
    {
        var path = Path.Combine(Environment.CurrentDirectory, $"{Guid.NewGuid():N}.db");

        _database = new SQLiteDatabase(new SQLiteRepositoryOptions
        {
            ConnectionString = path,
        });
    }

    public IDatabaseCollection<int, SQLiteDbItem> Collection =>
        _database.GetCollection<int, SQLiteDbItem>();

    public async Task InitializeAsync()
    {
        await _database.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _database.DeleteAsync();
    }

    public int GenerateId()
    {
        return Interlocked.Increment(ref _count);
    }
}