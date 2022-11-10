using System;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using SQLite;

namespace ManagedCode.Database.SQLite;

public class SqLiteDatabase : BaseDatabase, IDatabase<SQLiteConnection>
{
    public SqLiteDatabase(
        SQLiteRepositoryOptions options)
    {
        DBClient = options.Connection ?? new SQLiteConnection(options.ConnectionString);
        IsInitialized = true;
    }

    public override Task Delete(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public SQLiteConnection DBClient { get; }

    protected override ValueTask DisposeAsyncInternal()
    {
        DBClient.Close();
        DBClient.Dispose();
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
        DBClient.Close();
        DBClient.Dispose();
    }

    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public SQLiteDBCollection<TId, TItem> GetCollection<TId, TItem>() where TItem : class, IItem<TId>, new()
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }

        return new SQLiteDBCollection<TId, TItem>(DBClient);
    }
}