using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;

namespace ManagedCode.Database.LiteDB;

public class LiteDbDatabase : BaseDatabase, IDatabase<LiteDatabase>
{
    public LiteDbDatabase([NotNull] LiteDbRepositoryOptions options)
    {
        DataBase = options.Database ?? new LiteDatabase(options.ConnectionString);
        IsInitialized = true;
    }

    public override Task Delete(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public LiteDatabase DataBase { get; }

    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        DataBase.Dispose();
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
        DataBase.Dispose();
    }

    public LiteDbDBCollection<TId, TItem> GetCollection<TId, TItem>() where TItem : LiteDbItem<TId>, new()
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }

        return new LiteDbDBCollection<TId, TItem>(DataBase.GetCollection<TItem>());
    }
}