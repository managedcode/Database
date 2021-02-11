using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Repository.Core
{
    public abstract class BaseRepository<TId, TItem> : IRepository<TId, TItem> where TItem : IRepositoryItem<TId>
    {
        public bool IsInitialized { get; protected set; }

        public Task InitializeAsync(CancellationToken token = default)
        {
            if (IsInitialized == false)
            {
                return InitializeAsyncInternal(token);
            }

            return Task.CompletedTask;
        }

        protected abstract Task InitializeAsyncInternal(CancellationToken token = default);

        #region Insert

        public Task<bool> InsertAsync(TItem item, CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(item != null);
            return InsertAsyncInternal(item, token);
        }

        public Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(items != null);
            return InsertAsyncInternal(items, token);
        }

        protected abstract Task<bool> InsertAsyncInternal(TItem item, CancellationToken token = default);

        protected abstract Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default);

        #endregion

        #region Update

        public Task<bool> UpdateAsync(TItem item, CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(item != null);
            return UpdateAsyncInternal(item, token);
        }

        public Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(items != null);
            return UpdateAsyncInternal(items, token);
        }

        protected abstract Task<bool> UpdateAsyncInternal(TItem items, CancellationToken token = default);

        protected abstract Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default);

        #endregion

        #region Delete

        public Task<bool> DeleteAsync(TId id, CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(id != null);
            return DeleteAsyncInternal(id, token);
        }

        public Task<bool> DeleteAsync(TItem item, CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(item != null);
            return DeleteAsyncInternal(item, token);
        }

        public Task<int> DeleteAsync(IEnumerable<TId> ids, CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(ids != null);
            return DeleteAsyncInternal(ids, token);
        }

        public Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(items != null);
            return DeleteAsyncInternal(items, token);
        }

        public Task<int> DeleteAsync(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            return DeleteAsyncInternal(predicate, token);
        }

        public Task<bool> DeleteAllAsync(CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
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

        public Task<bool> InsertOrUpdateAsync(TItem item, CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(item != null);
            return InsertOrUpdateAsyncInternal(item, token);
        }

        public Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items, CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(items != null);
            return InsertOrUpdateAsyncInternal(items, token);
        }

        protected abstract Task<bool> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default);

        protected abstract Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default);

        #endregion

        #region Get

        public Task<TItem> GetAsync(TId id, CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(id != null);
            return GetAsyncInternal(id, token);
        }

        public Task<TItem> GetAsync(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            return GetAsyncInternal(predicate, token);
        }

        protected abstract Task<TItem> GetAsyncInternal(TId id, CancellationToken token = default);
        protected abstract Task<TItem> GetAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default);

        #endregion

        #region Find

        public IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate, int? take = null, int skip = 0, CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            return FindAsyncInternal(predicate, take, skip, token);
        }

        public IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            Contract.Requires(orderBy != null);
            return FindAsyncInternal(predicate, orderBy, Order.By, take, skip, token);
        }

        public IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            Contract.Requires(orderBy != null);
            return FindAsyncInternal(predicate, orderBy, orderType, take, skip, token);
        }

        public IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Expression<Func<TItem, object>> thenBy,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            Contract.Requires(orderBy != null);
            Contract.Requires(thenBy != null);
            return FindAsyncInternal(predicate, orderBy, Order.By, thenBy, Order.By, take, skip, token);
        }

        public IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            Expression<Func<TItem, object>> thenBy,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            Contract.Requires(orderBy != null);
            Contract.Requires(thenBy != null);
            return FindAsyncInternal(predicate, orderBy, orderType, thenBy, Order.By, take, skip, token);
        }

        public IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            Expression<Func<TItem, object>> thenBy,
            Order thenType,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            Contract.Requires(orderBy != null);
            Contract.Requires(thenBy != null);
            return FindAsyncInternal(predicate, orderBy, orderType, thenBy, thenType, take, skip, token);
        }

        protected abstract IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            int? take = null,
            int skip = 0,
            CancellationToken token = default);

        protected abstract IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            int? take = null,
            int skip = 0,
            CancellationToken token = default);

        protected abstract IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            Expression<Func<TItem, object>> thenBy,
            Order thenType,
            int? take = null,
            int skip = 0,
            CancellationToken token = default);

        #endregion

        #region Count

        public Task<uint> CountAsync(CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            return CountAsyncInternal(token);
        }

        public Task<uint> CountAsync(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            return CountAsyncInternal(predicate, token);
        }

        protected abstract Task<uint> CountAsyncInternal(CancellationToken token = default);

        protected abstract Task<uint> CountAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default);

        #endregion
    }
}