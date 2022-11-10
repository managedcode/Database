using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.Core;

public abstract class BaseDatabase<T> : IDatabase<T>
{
    private bool _disposed;

    public bool IsInitialized { get; protected set; }
    public T NativeClient { get; protected set; } = default!;

    public async Task InitializeAsync(CancellationToken token = default)
    {
        if (IsInitialized is false)
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