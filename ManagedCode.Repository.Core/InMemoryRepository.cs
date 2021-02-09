using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ManagedCode.Repository.Core
{
    public class InMemoryRepository<TId, TItem> : BaseRepository<TId, TItem> where TItem : IRepositoryItem<TId>
    {
        private readonly Dictionary<TId, TItem> _storage = new();

        protected override Task InitializeAsyncInternal()
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        #region Insert

        protected override Task<bool> InsertAsyncInternal(TItem item)
        {
            lock (_storage)
            {
                if (!_storage.TryGetValue(item.Id, out var _))
                {
                    _storage.Add(item.Id, item);
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }

        protected override Task<int> InsertAsyncInternal(IEnumerable<TItem> items)
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

        protected override Task<bool> InsertOrUpdateAsyncInternal(TItem item)
        {
            lock (_storage)
            {
                _storage[item.Id] = item;
                return Task.FromResult(true);
            }
        }

        protected override Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items)
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

        protected override Task<bool> UpdateAsyncInternal(TItem item)
        {
            lock (_storage)
            {
                if (_storage.TryGetValue(item.Id, out var _))
                {
                    _storage[item.Id] = item;
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
        }

        protected override Task<int> UpdateAsyncInternal(IEnumerable<TItem> items)
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

        protected override Task<bool> DeleteAsyncInternal(TId id)
        {
            lock (_storage)
            {
                return Task.FromResult(_storage.Remove(id));
            }
        }

        protected override Task<bool> DeleteAsyncInternal(TItem item)
        {
            lock (_storage)
            {
                return Task.FromResult(_storage.Remove(item.Id));
            }
        }

        protected override Task<int> DeleteAsyncInternal(IEnumerable<TId> ids)
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

        protected override Task<int> DeleteAsyncInternal(IEnumerable<TItem> items)
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

        protected override Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate)
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

        protected override Task<bool> DeleteAllAsyncInternal()
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

        protected override Task<TItem> GetAsyncInternal(TId id)
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

        protected override Task<TItem> GetAsyncInternal(Expression<Func<TItem, bool>> predicate)
        {
            lock (_storage)
            {
                var item = _storage.Values.FirstOrDefault(predicate.Compile());
                return Task.FromResult(item);
            }
        }

        #endregion

        #region Find

        protected override async IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate, int? take = null, int skip = 0)
        {
            await Task.Yield();
            List<TItem> list;
            lock (_storage)
            {
                var items = _storage.Values.Where(predicate.Compile());

                if (skip != 0)
                {
                    items = items.Skip(skip);
                }

                if (take.HasValue)
                {
                    items = items.Take(take.Value);
                }

                list = items.ToList();
            }

            await Task.Yield();
            foreach (var item in list)
            {
                yield return item;
            }
        }

        protected override async IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            int? take = null,
            int skip = 0)
        {
            await Task.Yield();
            List<TItem> list;
            lock (_storage)
            {
                var items = _storage.Values
                    .Where(predicate.Compile());

                items = orderType == Order.By ? items.OrderBy(orderBy.Compile()) : items.OrderByDescending(orderBy.Compile());

                if (skip != 0)
                {
                    items = items.Skip(skip);
                }

                if (take.HasValue)
                {
                    items = items.Take(take.Value);
                }

                list = items.ToList();
            }

            await Task.Yield();
            foreach (var item in list)
            {
                yield return item;
            }
        }

        protected override async IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            Expression<Func<TItem, object>> thenBy,
            Order thenType,
            int? take = null,
            int skip = 0)
        {
            await Task.Yield();
            List<TItem> list;
            lock (_storage)
            {
                var items = _storage.Values
                    .Where(predicate.Compile());

                var orderedItems = orderType == Order.By ? items.OrderBy(orderBy.Compile()) : items.OrderByDescending(orderBy.Compile());
                items = thenType == Order.By ? orderedItems.ThenBy(thenBy.Compile()) : orderedItems.ThenByDescending(thenBy.Compile());

                if (skip != 0)
                {
                    items = items.Skip(skip);
                }

                if (take.HasValue)
                {
                    items = items.Take(take.Value);
                }

                list = items.ToList();
            }

            await Task.Yield();
            foreach (var item in list)
            {
                yield return item;
            }
        }

        #endregion

        #region Count

        protected override Task<uint> CountAsyncInternal()
        {
            lock (_storage)
            {
                return Task.FromResult(Convert.ToUInt32(_storage.Count));
            }
        }

        protected override Task<uint> CountAsyncInternal(Expression<Func<TItem, bool>> predicate)
        {
            lock (_storage)
            {
                var count = _storage.Values.Count(predicate.Compile());
                return Task.FromResult(Convert.ToUInt32(count));
            }
        }

        #endregion
    }
}