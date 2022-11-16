using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using ManagedCode.Database.Core;

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

        public ICollectionQueryable<TItem> Query => new LiteDBCollectionQueryable<TId, TItem>(GetDatabase());

        private ILiteCollection<TItem> GetDatabase()
        {
            return _collection;
        }

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
            return GetDatabase().FindById(new BsonValue(id));
        }

        #endregion

        #region Count

        public async Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return GetDatabase().Count();
        }

        #endregion

        #region Insert

        public async Task<TItem> InsertAsync(TItem item, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            var v = GetDatabase().Insert(item);
            return GetDatabase().FindById(v);
        }

        public async Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return GetDatabase().InsertBulk(items);
        }

        #endregion

        #region InsertOrUpdate

        public async Task<TItem> InsertOrUpdateAsync(TItem item, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            GetDatabase().Upsert(item);
            return GetDatabase().FindById(new BsonValue(item.Id));
        }

        public async Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return GetDatabase().Upsert(items);
        }

        #endregion

        #region Update

        public async Task<TItem> UpdateAsync(TItem item, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            if (GetDatabase().Update(item))
            {
                return GetDatabase().FindById(new BsonValue(item.Id));
            }

            return default;
        }

        public async Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return GetDatabase().Update(items);
        }

        #endregion

        #region Delete

        public async Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return GetDatabase().Delete(new BsonValue(id));
        }

        public async Task<bool> DeleteAsync(TItem item, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return GetDatabase().Delete(new BsonValue(item.Id));
        }

        public async Task<int> DeleteAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            var count = 0;
            var db = GetDatabase();
            foreach (var id in ids)
            {
                if (db.Delete(new BsonValue(id)))
                {
                    count++;
                }
            }

            return count;
        }

        public async Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            var count = 0;
            var db = GetDatabase();
            foreach (var item in items)
            {
                if (db.Delete(new BsonValue(item.Id)))
                {
                    count++;
                }
            }

            return count;
        }

        public async Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return GetDatabase().DeleteAll() > 0;
        }

        #endregion
    }
}