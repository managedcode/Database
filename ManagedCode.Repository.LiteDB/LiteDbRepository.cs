using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using ManagedCode.Repository.Core;

namespace ManagedCode.Repository.LiteDB
{
    public class LiteDbRepository<TId, TItem> : BaseRepository<TId, TItem> where TItem : class, IRepositoryItem<TId>
    {
        private readonly LiteDatabase _database;

        public LiteDbRepository(string connectionString)
        {
            _database = new LiteDatabase(connectionString);
        }
        
        public LiteDbRepository(LiteDatabase database)
        {
            _database = database;
        }

        private ILiteCollection<TItem> GetDatabase()
        {
            return _database.GetCollection<TItem>();
        }

        protected override Task InitializeAsyncInternal(CancellationToken token = default)
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        #region Insert

        protected override async Task<TItem> InsertAsyncInternal(TItem item, CancellationToken token = default)
        {
            try
            {
                await Task.Yield();
                var v =  GetDatabase().Insert(item);
                return GetDatabase().FindById(v);
            }
            catch (Exception e)
            {
                return null;
            }
        }

        protected override async Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            try
            {
                await Task.Yield();
                return GetDatabase().InsertBulk(items);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        #endregion

        #region InsertOrUpdate

        protected override async Task<bool> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().Upsert(item);
        }

        protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().Upsert(items);
        }

        #endregion

        #region Update

        protected override async Task<bool> UpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().Update(item);
        }

        protected override async Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().Update(items);
        }

        #endregion

        #region Delete

        protected override async Task<bool> DeleteAsyncInternal(TId id, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().Delete(new BsonValue(id));
        }

        protected override async Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().Delete(new BsonValue(item.Id));
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TId> ids, CancellationToken token = default)
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

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
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

        protected override async Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().DeleteMany(predicate);
        }

        protected override async Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().DeleteAll() > 0;
        }

        #endregion

        #region Get

        protected override async Task<TItem> GetAsyncInternal(TId id, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().FindById(new BsonValue(id));
        }

        protected override async Task<TItem> GetAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            await Task.Yield();
            return GetDatabase().FindOne(predicate);
        }

        #endregion

        #region Find

        protected override async IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            int? take = null,
            int skip = 0,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            await Task.Yield();
            var query = GetDatabase().Find(predicate, skip, take ?? 2147483647);
            foreach (var item in query)
            {
                if(token.IsCancellationRequested)
                    break;
                
                yield return item;
            }
        }

        protected override async IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            int? take = null,
            int skip = 0,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            await Task.Yield();
            var query = GetDatabase().Query().Where(predicate);

            if (orderType == Order.By)
            {
                query = query.OrderBy(orderBy);
            }
            else
            {
                query.OrderByDescending(orderBy);
            }

            if (take != null)
            {
                foreach (var item in query.Limit(take.Value).ToEnumerable())
                {
                    if(token.IsCancellationRequested)
                        break;
                    
                    yield return item;
                }
            }
            else
            {
                foreach (var item in query.ToEnumerable())
                {
                    if(token.IsCancellationRequested)
                        break;
                    
                    yield return item;
                }
            }
        }

        protected override async IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            Expression<Func<TItem, object>> thenBy,
            Order thenType,
            int? take = null,
            int skip = 0,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            await Task.Yield();
            var query = GetDatabase().Query().Where(predicate);

            if (orderType == Order.By)
            {
                query = query.OrderBy(orderBy);
            }
            else
            {
                query.OrderByDescending(orderBy);
            }

            if (thenType == Order.By)
            {
                query = query.OrderBy(thenBy);
            }
            else
            {
                query.OrderByDescending(thenBy);
            }

            if (take != null)
            {
                foreach (var item in query.Limit(take.Value).ToEnumerable())
                {
                    yield return item;
                }
            }
            else
            {
                foreach (var item in query.ToEnumerable())
                {
                    yield return item;
                }
            }
        }

        #endregion

        #region Count

        protected override async Task<uint> CountAsyncInternal(CancellationToken token = default)
        {
            await Task.Yield();
            return (uint) GetDatabase().Count();
        }

        protected override async Task<uint> CountAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            await Task.Yield();
            return (uint) GetDatabase().Count(predicate);
        }

        #endregion
    }
}