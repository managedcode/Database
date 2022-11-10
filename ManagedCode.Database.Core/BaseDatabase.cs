using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core.Common;

namespace ManagedCode.Database.Core;

public abstract class BaseDatabase<T> : IDatabase<T>
{
    private bool _disposed;
    private T _nativeClient = default!;

    public bool IsInitialized { get; protected set; }

    public T NativeClient
    {
        get
        {
            if (!IsInitialized)
            {
                throw new DatabaseNotInitializedException(GetType());
            }

            return _nativeClient;
        }

        protected set => _nativeClient = value;
    }

    public async Task InitializeAsync(CancellationToken token = default)
    {
        if (!IsInitialized)
        {
            await InitializeAsyncInternal(token);

            IsInitialized = true;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        DisposeInternal();
    }

    public ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return new ValueTask(Task.CompletedTask);
        }

        _disposed = true;
        return DisposeAsyncInternal();
    }

    protected abstract Task InitializeAsyncInternal(CancellationToken token = default);

    protected abstract ValueTask DisposeAsyncInternal();
    protected abstract void DisposeInternal();
    public abstract Task DeleteAsync(CancellationToken token = default);
}