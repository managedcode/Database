using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.Core.InMemory;

public class InMemoryDBCollection<TId, TItem> : BaseDBCollection<TId, TItem> where TItem : IItem<TId>
{
    private readonly Dictionary<TId, TItem> _storage = new ();
    
    public override void Dispose()
    {
        lock (_storage)
        {
            _storage.Clear();
        }
    }

    public override ValueTask DisposeAsync()
    {
        Dispose();
        return new ValueTask(Task.CompletedTask);
    }

    #region Insert

    protected override Task<TItem> InsertAsyncInternal(TItem item, CancellationToken token = default)
    {
        lock (_storage)
        {
            if (!_storage.TryGetValue(item.Id, out var _))
            {
                _storage.Add(item.Id, item);
                return Task.FromResult(item);
            }
        }

        return Task.FromResult<TItem>(default);
    }

    protected override Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = 0;

        lock (_storage)
        {
            foreach (var item in items)
            {
                if (!_storage.TryGetValue(item.Id, out var _))
                {
                    _storage.Add(item.Id, item);
                    count++;
                }
            }
        }

        return Task.FromResult(count);
    }

    #endregion

    #region InsertOrUpdate

    protected override Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        lock (_storage)
        {
            _storage[item.Id] = item;
            return Task.FromResult(item);
        }
    }

    protected override Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = 0;

        lock (_storage)
        {
            foreach (var item in items)
            {
                _storage[item.Id] = item;
                count++;
            }
        }

        return Task.FromResult(count);
    }

    #endregion

    #region Update

    protected override Task<TItem> UpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        lock (_storage)
        {
            if (_storage.TryGetValue(item.Id, out var _))
            {
                _storage[item.Id] = item;
                return Task.FromResult(item);
            }

            return Task.FromResult<TItem>(default);
        }
    }

    protected override Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = 0;

        lock (_storage)
        {
            foreach (var item in items)
            {
                if (_storage.TryGetValue(item.Id, out var _))
                {
                    _storage[item.Id] = item;
                    count++;
                }
            }
        }

        return Task.FromResult(count);
    }

    #endregion

    #region Delete

    protected override Task<bool> DeleteAsyncInternal(TId id, CancellationToken token = default)
    {
        lock (_storage)
        {
            return Task.FromResult(_storage.Remove(id));
        }
    }

    protected override Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
    {
        lock (_storage)
        {
            return Task.FromResult(_storage.Remove(item.Id));
        }
    }

    protected override Task<int> DeleteAsyncInternal(IEnumerable<TId> ids, CancellationToken token = default)
    {
        var count = 0;

        lock (_storage)
        {
            foreach (var id in ids)
            {
                if (_storage.Remove(id))
                {
                    count++;
                }
            }
        }

        return Task.FromResult(count);
    }

    protected override Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = 0;

        lock (_storage)
        {
            foreach (var item in items)
            {
                if (_storage.Remove(item.Id))
                {
                    count++;
                }
            }
        }

        return Task.FromResult(count);
    }

    protected override Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
    {
        lock (_storage)
        {
            var count = 0;
            var items = _storage.Values.Where(predicate.Compile());
            foreach (var item in items)
            {
                if (_storage.Remove(item.Id))
                {
                    count++;
                }
            }

            return Task.FromResult(count);
        }
    }

    protected override Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
    {
        lock (_storage)
        {
            var count = _storage.Count;
            _storage.Clear();
            return Task.FromResult(true);
        }
    }

    #endregion

    #region Get

    protected override Task<TItem> GetAsyncInternal(TId id, CancellationToken token = default)
    {
        lock (_storage)
        {
            if (_storage.TryGetValue(id, out var item))
            {
                return Task.FromResult(item);
            }

            return Task.FromResult<TItem>(default);
        }
    }
    
    #endregion
    

    #region Count

    public override IDBCollectionQueryable<TItem> Query()
    {
        lock (_storage)
        {
            return new InMemoryDBCollectionQueryable<TId,TItem>(_storage);
        }
    }

    protected override Task<long> CountAsyncInternal(CancellationToken token = default)
    {
        lock (_storage)
        {
            return Task.FromResult((long)_storage.Count);
        }
    }
    
    #endregion
}

