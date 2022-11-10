using System;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;

namespace ManagedCode.Database.LiteDB;

public class LiteDbDatabase : BaseDatabase<LiteDatabase>
{
    public LiteDbDatabase(LiteDbRepositoryOptions options)
    {
        NativeClient = options.Database ?? new LiteDatabase(options.ConnectionString);
        IsInitialized = true;
    }

    public override Task DeleteAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }


    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        NativeClient.Dispose();
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
        NativeClient.Dispose();
    }

    public LiteDbDBCollection<TId, TItem> GetCollection<TId, TItem>() where TItem : LiteDbItem<TId>, new()
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }

        return new LiteDbDBCollection<TId, TItem>(NativeClient.GetCollection<TItem>());
    }
}