using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Exceptions;

namespace ManagedCode.Database.LiteDB
{
    public class LiteDBCollection<TId, TItem> : IDatabaseCollection<TId, TItem>
        where TItem : LiteDBItem<TId>, IItem<TId>, new()
    {
        private readonly ILiteCollection<TItem> _collection;

        public LiteDBCollection(ILiteCollection<TItem> collection)
        {
            _collection = collection;
        }

        public ICollectionQueryable<TItem> Query => new LiteDBCollectionQueryable<TId, TItem>(_collection);

        public ValueTask DisposeAsync()
        {
            return new ValueTask(Task.CompletedTask);
        }

        public void Dispose()
        {
        }

        #region Get

        public async Task<TItem?> GetAsync(TId id, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return ExceptionCatcher.Execute(() => _collection.FindById(new BsonValue(id)));
        }

        #endregion

        #region Count

        public async Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            var task = Task.Run(() => _collection.Count(), cancellationToken);
            return await ExceptionCatcher.ExecuteAsync(task);
        }

        #endregion

        #region Insert

        public async Task<TItem> InsertAsync(TItem item, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return ExceptionCatcher.Execute(() =>
            {
                var bson = _collection.Insert(item);
                return _collection.FindById(bson);
            });
        }

        public async Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
        {
            var task = Task.Run(() => _collection.InsertBulk(items), cancellationToken);
            return await ExceptionCatcher.ExecuteAsync(task);
        }

        #endregion

        #region InsertOrUpdate

        public async Task<TItem> InsertOrUpdateAsync(TItem item, CancellationToken cancellationToken = default)
        {
            var task = Task.Run(() =>
            {
                _collection.Upsert(item);
                return _collection.FindById(new BsonValue(item.Id));
            }, cancellationToken);

            return await ExceptionCatcher.ExecuteAsync(task);
        }

        public async Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items,
            CancellationToken cancellationToken = default)
        {
            var task = Task.Run(() => _collection.Upsert(items), cancellationToken);
            return await ExceptionCatcher.ExecuteAsync(task);
        }

        #endregion

        #region Update

        public async Task<TItem> UpdateAsync(TItem item, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            var isUpdated = ExceptionCatcher.Execute(() => _collection.Update(item));

            if (!isUpdated)
            {
                throw new DatabaseException("Entity not found in collection.");
            }

            return _collection.FindById(new BsonValue(item.Id));
        }

        public async Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return ExceptionCatcher.Execute(() => _collection.Update(items));
        }

        #endregion

        #region Delete

        public async Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return ExceptionCatcher.Execute(() => _collection.Delete(new BsonValue(id)));
        }

        public async Task<bool> DeleteAsync(TItem item, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return ExceptionCatcher.Execute(() => _collection.Delete(new BsonValue(item.Id)));
        }

        public async Task<int> DeleteAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
        {
            var task = Task.Run(() => ids.Count(id => _collection.Delete(new BsonValue(id))), cancellationToken);
            return await ExceptionCatcher.ExecuteAsync(task);
        }

        public async Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
        {
            var task = Task.Run(() => items.Count(item => _collection.Delete(new BsonValue(item.Id))),
                cancellationToken);

            return await ExceptionCatcher.ExecuteAsync(task);
        }

        public async Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default)
        {
            var task = Task.Run(() => _collection.DeleteAll(), cancellationToken);
            var count = await ExceptionCatcher.ExecuteAsync(task);
            return count > 0;
        }

        #endregion
    }
}