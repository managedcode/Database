using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core.Common;

namespace ManagedCode.Database.Core.InMemory
{
    public class InMemoryDatabase : BaseDatabase<ConcurrentDictionary<string, IDisposable>>
    {
        protected override Task InitializeAsyncInternal(CancellationToken token = default)
        {
            NativeClient = new ConcurrentDictionary<string, IDisposable>();

            return Task.CompletedTask;
        }

        protected override ValueTask DisposeAsyncInternal()
        {
            DisposeInternal();
            return new ValueTask(Task.CompletedTask);
        }

        protected override void DisposeInternal()
        {
            foreach (var item in NativeClient)
            {
                item.Value.Dispose();
            }

            NativeClient.Clear();
        }

        public InMemoryDatabaseCollection<TId, TItem> GetCollection<TId, TItem>() where TItem : IItem<TId>
        {
            return GetCollection<TId, TItem>(typeof(TItem).FullName);
        }

        public InMemoryDatabaseCollection<TId, TItem> GetCollection<TId, TItem>(string name) where TItem : IItem<TId>
        {
            if (!IsInitialized)
            {
                throw new DatabaseNotInitializedException(GetType());
            }
        
            if (NativeClient.TryGetValue(name, out var table))
            {
                return (InMemoryDatabaseCollection<TId, TItem>)table;
            }

            var db = new InMemoryDatabaseCollection<TId, TItem>();
            NativeClient[name] = db;
            return db;
        }

        public override Task DeleteAsync(CancellationToken token = default)
        {
            DisposeInternal();
            return Task.CompletedTask;
        }
    }
}