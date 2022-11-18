using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core.Exceptions;

namespace ManagedCode.Database.Core;

public abstract class BaseDatabaseCollection<TId, TItem> : IDatabaseCollection<TId, TItem>
    where TItem : IItem<TId>
{
    public abstract ICollectionQueryable<TItem> Query { get; }

    public abstract void Dispose();

    public abstract ValueTask DisposeAsync();

    public Task<TItem> InsertAsync(TItem item, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(InsertInternalAsync(item, cancellationToken));
    }

    public Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(InsertInternalAsync(items, cancellationToken));
    }

    public Task<TItem> UpdateAsync(TItem item, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(UpdateInternalAsync(item, cancellationToken));
    }

    public Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(UpdateInternalAsync(items, cancellationToken));
    }

    public Task<TItem> InsertOrUpdateAsync(TItem item, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(InsertOrUpdateInternalAsync(item, cancellationToken));
    }

    public Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(InsertOrUpdateInternalAsync(items, cancellationToken));
    }

    public Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(DeleteInternalAsync(id, cancellationToken));
    }

    public Task<bool> DeleteAsync(TItem item, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(DeleteInternalAsync(item, cancellationToken));
    }

    public Task<int> DeleteAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(DeleteInternalAsync(ids, cancellationToken));
    }

    public Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(DeleteInternalAsync(items, cancellationToken));
    }

    public Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(DeleteCollectionInternalAsync(cancellationToken));
    }

    public Task<TItem?> GetAsync(TId id, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(GetInternalAsync(id, cancellationToken));
    }

    public Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(CountInternalAsync(cancellationToken));
    }

    protected abstract Task<TItem> InsertInternalAsync(TItem item, CancellationToken cancellationToken = default);

    protected abstract Task<int> InsertInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default);

    protected abstract Task<TItem> UpdateInternalAsync(TItem item, CancellationToken cancellationToken = default);

    protected abstract Task<int> UpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default);

    protected abstract Task<TItem> InsertOrUpdateInternalAsync(TItem item,
        CancellationToken cancellationToken = default);

    protected abstract Task<int> InsertOrUpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default);

    protected abstract Task<bool> DeleteInternalAsync(TId id, CancellationToken cancellationToken = default);

    protected abstract Task<bool> DeleteInternalAsync(TItem item, CancellationToken cancellationToken = default);

    protected abstract Task<int> DeleteInternalAsync(IEnumerable<TId> ids,
        CancellationToken cancellationToken = default);

    protected abstract Task<int> DeleteInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default);

    protected abstract Task<bool> DeleteCollectionInternalAsync(CancellationToken cancellationToken = default);

    protected abstract Task<TItem?> GetInternalAsync(TId id, CancellationToken cancellationToken = default);

    protected abstract Task<long> CountInternalAsync(CancellationToken cancellationToken = default);

    private static async Task<T> ExecuteAsync<T>(Task<T> task)
    {
        try
        {
            return await task;
        }
        catch (Exception exception) when (exception is not NotImplementedException or DatabaseException)
        {
            throw new DatabaseException(exception.Message, exception);
        }
    }
}