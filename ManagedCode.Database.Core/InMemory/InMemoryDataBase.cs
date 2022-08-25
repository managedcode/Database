using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.Core.InMemory;

public class InMemoryDataBase : BaseDatabase, IDatabase<Dictionary<string, IDisposable>>
{
    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        DisposeInternal();
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
        foreach (var item in DataBase)
        {
            item.Value.Dispose();
        }
        DataBase.Clear();
    }

    protected override IDBCollection<TId, TItem> GetCollectionInternal<TId, TItem>(string name)
    {
        lock (DataBase)
        {
            if (DataBase.TryGetValue(name, out var table))
            {
                return (IDBCollection<TId, TItem>)table;
            }

            var db = new InMemoryDBCollection<TId, TItem>();
            DataBase[name] = db;
            return db;
        }

    }

    public override Task Delete(CancellationToken token = default)
    {
        DisposeInternal();
        return Task.CompletedTask;
    }

    public Dictionary<string, IDisposable> DataBase { get; } = new();
}