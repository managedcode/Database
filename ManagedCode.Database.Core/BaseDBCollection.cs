using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core.Queries;

namespace ManagedCode.Database.Core;

public abstract class BaseDBCollection<TId, TItem> : IDBCollection<TId, TItem> where TItem : IItem<TId>
{
    public abstract void Dispose();
    public abstract ValueTask DisposeAsync();

    #region Insert

    public Task<TItem> InsertAsync(TItem item, CancellationToken token = default)
    {

        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        return InsertAsyncInternal(item, token);
    }

    public Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        return InsertAsyncInternal(items, token);
    }

    protected abstract Task<TItem> InsertAsyncInternal(TItem item, CancellationToken token = default);

    protected abstract Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default);

    #endregion

    #region Update

    public Task<TItem> UpdateAsync(TItem item, CancellationToken token = default)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        return UpdateAsyncInternal(item, token);
    }

    public Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        return UpdateAsyncInternal(items, token);
    }

    protected abstract Task<TItem> UpdateAsyncInternal(TItem item, CancellationToken token = default);

    protected abstract Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default);

    #endregion

    #region Delete

    public Task<bool> DeleteAsync(TId id, CancellationToken token = default)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return DeleteAsyncInternal(id, token);
    }

    public Task<bool> DeleteAsync(TItem item, CancellationToken token = default)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        return DeleteAsyncInternal(item, token);
    }

    public Task<int> DeleteAsync(IEnumerable<TId> ids, CancellationToken token = default)
    {
        if (ids == null)
        {
            throw new ArgumentNullException(nameof(ids));
        }

        return DeleteAsyncInternal(ids, token);
    }

    public Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        return DeleteAsyncInternal(items, token);
    }

    public Task<int> DeleteAsync(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
    {
        if (predicate == null)
        {
            throw new ArgumentNullException(nameof(predicate));
        }

        return DeleteAsyncInternal(predicate, token);
    }

    public Task<bool> DeleteAllAsync(CancellationToken token = default)
    {
        return DeleteAllAsyncInternal(token);
    }

    protected abstract Task<bool> DeleteAsyncInternal(TId id, CancellationToken token = default);

    protected abstract Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default);

    protected abstract Task<int> DeleteAsyncInternal(IEnumerable<TId> ids, CancellationToken token = default);

    protected abstract Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default);

    protected abstract Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default);

    protected abstract Task<bool> DeleteAllAsyncInternal(CancellationToken token = default);

    #endregion

    #region InsertOrUpdate

    public Task<TItem> InsertOrUpdateAsync(TItem item, CancellationToken token = default)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        return InsertOrUpdateAsyncInternal(item, token);
    }

    public Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {

        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        return InsertOrUpdateAsyncInternal(items, token);
    }

    protected abstract Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default);

    protected abstract Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default);

    #endregion

    #region Get

    public Task<TItem> GetAsync(TId id, CancellationToken token = default)
    {
        if (id == null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return GetAsyncInternal(id, token);
    }

    protected abstract Task<TItem> GetAsyncInternal(TId id, CancellationToken token = default);

    #endregion

    #region Count

    public Task<long> CountAsync(CancellationToken token = default)
    {
        return CountAsyncInternal(token);
    }

    public abstract IDBCollectionQueryable<TItem> Query();

    protected abstract Task<long> CountAsyncInternal(CancellationToken token = default);

    #endregion
}