using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.Core.InMemory;

public class InMemoryDBCollection<TId, TItem> : IDBCollection<TId, TItem> where TItem : IItem<TId> where TId : notnull
{
    private readonly ConcurrentDictionary<TId, TItem> _storage = new();

    public IDBCollectionQueryable<TItem> Query => new InMemoryDBCollectionQueryable<TId, TItem>(_storage);

    public void Dispose()
    {
        _storage.Clear();
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return new ValueTask(Task.CompletedTask);
    }

    #region Insert

    public Task<TItem> InsertAsync(TItem item, CancellationToken token = default)
    {
        if (!_storage.TryGetValue(item.Id, out _))
        {
            _storage.TryAdd(item.Id, item);
            return Task.FromResult(item);
        }

        return Task.FromResult<TItem>(default);
    }

    public Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = 0;


        foreach (var item in items)
        {
            if (!_storage.TryGetValue(item.Id, out var _))
            {
                _storage.TryAdd(item.Id, item);
                count++;
            }
        }


        return Task.FromResult(count);
    }

    #endregion

    #region InsertOrUpdate

    public Task<TItem> InsertOrUpdateAsync(TItem item, CancellationToken token = default)
    {
        _storage[item.Id] = item;
        return Task.FromResult(item);
    }

    public Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items,
        CancellationToken token = default)
    {
        var count = 0;

        foreach (var item in items)
        {
            _storage[item.Id] = item;
            count++;
        }

        return Task.FromResult(count);
    }

    #endregion

    #region Update

    public Task<TItem> UpdateAsync(TItem item, CancellationToken token = default)
    {
        if (_storage.TryGetValue(item.Id, out _))
        {
            _storage[item.Id] = item;
            return Task.FromResult(item);
        }

        return Task.FromResult<TItem>(default);
    }

    public Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = 0;

        foreach (var item in items)
        {
            if (_storage.TryGetValue(item.Id, out var _))
            {
                _storage[item.Id] = item;
                count++;
            }
        }

        return Task.FromResult(count);
    }

    #endregion

    #region Delete

    public Task<bool> DeleteAsync(TId id, CancellationToken token = default)
    {
        return Task.FromResult(_storage.TryRemove(id, out _));
    }

    public Task<bool> DeleteAsync(TItem item, CancellationToken token = default)
    {
        return Task.FromResult(_storage.TryRemove(item.Id, out _));
    }

    public Task<int> DeleteAsync(IEnumerable<TId> ids, CancellationToken token = default)
    {
        var count = ids.Count(id => _storage.TryRemove(id, out _));

        return Task.FromResult(count);
    }

    public Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = items.Count(item => _storage.TryRemove(item.Id, out _));

        return Task.FromResult(count);
    }

    public Task<bool> DeleteCollectionAsync(CancellationToken token = default)
    {
        _storage.Clear();
        return Task.FromResult(_storage.Count == 0);
    }

    #endregion

    #region Get

    public Task<TItem> GetAsync(TId id, CancellationToken token = default)
    {
        if (_storage.TryGetValue(id, out var item))
        {
            return Task.FromResult(item);
        }

        return Task.FromResult<TItem>(default);
    }

    #endregion


    #region Count

    public Task<long> CountAsync(CancellationToken token = default)
    {
        return Task.FromResult((long)_storage.Count);
    }

    #endregion
}