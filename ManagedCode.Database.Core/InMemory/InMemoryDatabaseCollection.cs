using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core.Exceptions;

namespace ManagedCode.Database.Core.InMemory
{
    public class InMemoryDatabaseCollection<TId, TItem> : IDatabaseCollection<TId, TItem>
        where TItem : IItem<TId> where TId : notnull
    {
        private readonly ConcurrentDictionary<TId, TItem> _storage = new();

        public ICollectionQueryable<TItem> Query => new InMemoryCollectionQueryable<TId, TItem>(_storage);

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

        public Task<TItem> InsertAsync(TItem item, CancellationToken cancellationToken = default)
        {
            if (!_storage.TryGetValue(item.Id, out _))
            {
                _storage.TryAdd(item.Id, item);
                return Task.FromResult(item);
            }

            throw new DatabaseException("The specified entity already exists.");
        }

        public Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
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

        public Task<TItem> InsertOrUpdateAsync(TItem item, CancellationToken cancellationToken = default)
        {
            _storage[item.Id] = item;
            return Task.FromResult(item);
        }

        public Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items,
            CancellationToken cancellationToken = default)
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

        public Task<TItem> UpdateAsync(TItem item, CancellationToken cancellationToken = default)
        {
            if (_storage.TryGetValue(item.Id, out _))
            {
                _storage[item.Id] = item;
                return Task.FromResult(item);
            }

            throw new DatabaseException("Entity not found in collection.");
        }

        public Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
        {
            var count = 0;

            foreach (var item in items)
            {
                if (_storage.TryGetValue(item.Id, out _))
                {
                    _storage[item.Id] = item;
                    count++;
                }
            }

            return Task.FromResult(count);
        }

        #endregion

        #region Delete

        public Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_storage.TryRemove(id, out _));
        }

        public Task<bool> DeleteAsync(TItem item, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_storage.TryRemove(item.Id, out _));
        }

        public Task<int> DeleteAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
        {
            var count = ids.Count(id => _storage.TryRemove(id, out _));

            return Task.FromResult(count);
        }

        public Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
        {
            var count = items.Count(item => _storage.TryRemove(item.Id, out _));

            return Task.FromResult(count);
        }

        public Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default)
        {
            _storage.Clear();
            return Task.FromResult(_storage.Count == 0);
        }

        #endregion

        #region Get

        public Task<TItem> GetAsync(TId id, CancellationToken cancellationToken = default)
        {
            if (_storage.TryGetValue(id, out var item))
            {
                return Task.FromResult(item);
            }

            return Task.FromResult<TItem>(default);
        }

        #endregion


        #region Count

        public Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((long)_storage.Count);
        }

        #endregion
    }
}