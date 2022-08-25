using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.Core;

public interface IDatabase : IDisposable, IAsyncDisposable
{
    bool IsInitialized { get; }

    Task InitializeAsync(CancellationToken token = default);
    
    Task Delete(CancellationToken token = default);

    IDBCollection<TId, TItem> GetCollection<TId, TItem>() where TItem : class, IItem<TId>, new();
    IDBCollection<TId, TItem> GetCollection<TId, TItem>(string name) where TItem : class, IItem<TId>, new();
}

public interface IDatabase<out T> : IDatabase
{
    T DataBase { get; }
}