using System;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using SQLite;

namespace ManagedCode.Database.SQLite;

public class SqLiteDatabase : BaseDatabase<SQLiteConnection>
{
    public SqLiteDatabase(
        SQLiteRepositoryOptions options)
    {
        NativeClient = options.Connection ?? new SQLiteConnection(options.ConnectionString);
        IsInitialized = true;
    }

    public override Task DeleteAsync(CancellationToken token = default)
    {
        DisposeInternal();
        System.IO.File.Delete(NativeClient.DatabasePath);
        return Task.CompletedTask;
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        NativeClient.Close();
        NativeClient.Dispose();
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
        NativeClient.Close();
        NativeClient.Dispose();
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

        NativeClient.CreateTable<TItem>();
        return new SQLiteDBCollection<TId, TItem>(NativeClient);
    }
}