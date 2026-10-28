using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core.Exceptions;

namespace ManagedCode.Database.Core.InMemory
{
    public class InMemoryDatabaseCollection<TId, TItem> : BaseDatabaseCollection<TId, TItem>
        where TItem : IItem<TId> where TId : notnull
    {
        private readonly ConcurrentDictionary<TId, TItem> _storage = new();

        public override ICollectionQueryable<TItem> Query => new InMemoryCollectionQueryable<TId, TItem>(_storage);

        public override void Dispose()
        {
            _storage.Clear();
        }

        public override ValueTask DisposeAsync()
        {
            Dispose();
            return new ValueTask(Task.CompletedTask);
        }

        #region Insert

        protected override Task<TItem> InsertInternalAsync(TItem item, CancellationToken cancellationToken = default)
        {
            if (!_storage.TryGetValue(item.Id, out _))
            {
                _storage.TryAdd(item.Id, item);
                return Task.FromResult(item);
            }

            throw new DatabaseException("The specified entity already exists.");
        }

        protected override Task<int> InsertInternalAsync(IEnumerable<TItem> items,
            CancellationToken cancellationToken = default)
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

        protected override Task<TItem> InsertOrUpdateInternalAsync(TItem item,
            CancellationToken cancellationToken = default)
        {
            _storage[item.Id] = item;
            return Task.FromResult(item);
        }

        protected override Task<int> InsertOrUpdateInternalAsync(IEnumerable<TItem> items,
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

        protected override Task<TItem> UpdateInternalAsync(TItem item, CancellationToken cancellationToken = default)
        {
            if (_storage.TryGetValue(item.Id, out _))
            {
                _storage[item.Id] = item;
                return Task.FromResult(item);
            }

            throw new DatabaseException("Entity not found in collection.");
        }

        protected override Task<int> UpdateInternalAsync(IEnumerable<TItem> items,
            CancellationToken cancellationToken = default)
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

        protected override Task<bool> DeleteInternalAsync(TId id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_storage.TryRemove(id, out _));
        }

        protected override Task<bool> DeleteInternalAsync(TItem item, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_storage.TryRemove(item.Id, out _));
        }

        protected override Task<int> DeleteInternalAsync(IEnumerable<TId> ids,
            CancellationToken cancellationToken = default)
        {
            var count = ids.Count(id => _storage.TryRemove(id, out _));

            return Task.FromResult(count);
        }

        protected override Task<int> DeleteInternalAsync(IEnumerable<TItem> items,
            CancellationToken cancellationToken = default)
        {
            var count = items.Count(item => _storage.TryRemove(item.Id, out _));

            return Task.FromResult(count);
        }

        protected override Task<bool> DeleteCollectionInternalAsync(CancellationToken cancellationToken = default)
        {
            _storage.Clear();
            return Task.FromResult(_storage.Count == 0);
        }

        

        #endregion

        #region Get
        
        protected override Task<List<TItem>> GetCollectionInternalAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_storage.Values.ToList());
        }

        protected override Task<TItem> GetInternalAsync(TId id, CancellationToken cancellationToken = default)
        {
            if (_storage.TryGetValue(id, out var item))
            {
                return Task.FromResult(item);
            }

            return Task.FromResult<TItem>(default);
        }

        #endregion


        #region Count

        protected override Task<long> CountInternalAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult((long)_storage.Count);
        }

        #endregion
    }
}