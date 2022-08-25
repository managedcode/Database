using System;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using SQLite;

namespace ManagedCode.Database.SQLite;

public class SqLiteDatabase : BaseDatabase, IDatabase<SQLiteConnection>
{
    public SqLiteDatabase(
        [System.Diagnostics.CodeAnalysis.NotNull]
        SQLiteRepositoryOptions options)
    {
        DataBase = options.Connection ?? new SQLiteConnection(options.ConnectionString);
        IsInitialized = true;
    }

    
    protected override ValueTask DisposeAsyncInternal()
    {
        DataBase.Close();
        DataBase.Dispose();
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
        DataBase.Close();
        DataBase.Dispose();
    }
    
    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    protected override IDBCollection<TId, TItem> GetCollectionInternal<TId, TItem>(string name)
    {
        DataBase.CreateTable<TItem>();
        return new SQLiteDBCollection<TId, TItem>(DataBase);
    }

    public override Task Delete(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public SQLiteConnection DataBase { get; }
}