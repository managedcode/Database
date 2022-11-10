using System;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.Core;

public interface IDatabase : IDisposable, IAsyncDisposable
{
    bool IsInitialized { get; }

    Task InitializeAsync(CancellationToken token = default);

    Task DeleteAsync(CancellationToken token = default);
}

public interface IDatabase<out T> : IDatabase
{
    T NativeClient { get; }
}