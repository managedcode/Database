using System;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;

namespace ManagedCode.Database.LiteDB;

public class LiteDBDatabase : BaseDatabase<LiteDatabase>
{
    private readonly LiteDBOptions _options;

    public LiteDBDatabase(LiteDBOptions options)
    {
        _options = options;
    }

    public override Task DeleteAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }


    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        NativeClient = _options.Database ?? new LiteDatabase(_options.ConnectionString);

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

    public LiteDBCollection<TId, TItem> GetCollection<TId, TItem>() where TItem : LiteDBItem<TId>, new()
    {
        if (!IsInitialized) throw new DatabaseNotInitializedException(GetType());

        return new LiteDBCollection<TId, TItem>(NativeClient.GetCollection<TItem>());
    }
}