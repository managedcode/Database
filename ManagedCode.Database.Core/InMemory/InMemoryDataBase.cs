using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core.Common;

namespace ManagedCode.Database.Core.InMemory;

public class InMemoryDataBase : BaseDatabase<Dictionary<string, IDisposable>>
{
    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        NativeClient = new Dictionary<string, IDisposable>();

        return Task.CompletedTask;
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        DisposeInternal();
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
        lock (NativeClient)
        {
            foreach (var item in NativeClient)
            {
                item.Value.Dispose();
            }

            NativeClient.Clear();
        }
    }

    public InMemoryDBCollection<TId, TItem> GetCollection<TId, TItem>() where TItem : IItem<TId>
    {
        return GetCollection<TId, TItem>(typeof(TItem).FullName);
    }

    public InMemoryDBCollection<TId, TItem> GetCollection<TId, TItem>(string name) where TItem : IItem<TId>
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }

        lock (NativeClient)
        {
            if (NativeClient.TryGetValue(name, out var table))
            {
                return (InMemoryDBCollection<TId, TItem>)table;
            }

            var db = new InMemoryDBCollection<TId, TItem>();
            NativeClient[name] = db;
            return db;
        }
    }

    public override Task DeleteAsync(CancellationToken token = default)
    {
        DisposeInternal();
        return Task.CompletedTask;
    }
}