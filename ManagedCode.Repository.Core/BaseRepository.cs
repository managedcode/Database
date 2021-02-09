using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ManagedCode.Repository.Core
{
    public abstract class BaseRepository<TId, TItem> : IRepository<TId, TItem> where TItem : IRepositoryItem<TId>
    {
        public bool IsInitialized { get; protected set; }

        public Task InitializeAsync()
        {
            if (IsInitialized == false)
            {
                return InitializeAsyncInternal();
            }

            return Task.CompletedTask;
        }

        protected abstract Task InitializeAsyncInternal();

        #region Insert

        public Task<bool> InsertAsync(TItem item)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(item != null);
            return InsertAsyncInternal(item);
        }

        public Task<int> InsertAsync(IEnumerable<TItem> items)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(items != null);
            return InsertAsyncInternal(items);
        }

        protected abstract Task<bool> InsertAsyncInternal(TItem item);

        protected abstract Task<int> InsertAsyncInternal(IEnumerable<TItem> items);

        #endregion

        #region Update

        public Task<bool> UpdateAsync(TItem item)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(item != null);
            return UpdateAsyncInternal(item);
        }

        public Task<int> UpdateAsync(IEnumerable<TItem> items)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(items != null);
            return UpdateAsyncInternal(items);
        }

        protected abstract Task<bool> UpdateAsyncInternal(TItem items);

        protected abstract Task<int> UpdateAsyncInternal(IEnumerable<TItem> items);

        #endregion

        #region Delete

        public Task<bool> DeleteAsync(TId id)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(id != null);
            return DeleteAsyncInternal(id);
        }

        public Task<bool> DeleteAsync(TItem item)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(item != null);
            return DeleteAsyncInternal(item);
        }

        public Task<int> DeleteAsync(IEnumerable<TId> ids)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(ids != null);
            return DeleteAsyncInternal(ids);
        }

        public Task<int> DeleteAsync(IEnumerable<TItem> items)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(items != null);
            return DeleteAsyncInternal(items);
        }

        public Task<int> DeleteAsync(Expression<Func<TItem, bool>> predicate)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            return DeleteAsyncInternal(predicate);
        }

        public Task<bool> DeleteAllAsync()
        {
            Contract.Requires(IsInitialized);
            return DeleteAllAsyncInternal();
        }

        protected abstract Task<bool> DeleteAsyncInternal(TId id);

        protected abstract Task<bool> DeleteAsyncInternal(TItem item);

        protected abstract Task<int> DeleteAsyncInternal(IEnumerable<TId> ids);

        protected abstract Task<int> DeleteAsyncInternal(IEnumerable<TItem> items);

        protected abstract Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate);

        protected abstract Task<bool> DeleteAllAsyncInternal();

        #endregion

        #region InsertOrUpdate

        public Task<bool> InsertOrUpdateAsync(TItem item)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(item != null);
            return InsertOrUpdateAsyncInternal(item);
        }

        public Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(items != null);
            return InsertOrUpdateAsyncInternal(items);
        }

        protected abstract Task<bool> InsertOrUpdateAsyncInternal(TItem item);

        protected abstract Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items);

        #endregion

        #region Get

        public Task<TItem> GetAsync(TId id)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(id != null);
            return GetAsyncInternal(id);
        }

        public Task<TItem> GetAsync(Expression<Func<TItem, bool>> predicate)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            return GetAsyncInternal(predicate);
        }

        protected abstract Task<TItem> GetAsyncInternal(TId id);
        protected abstract Task<TItem> GetAsyncInternal(Expression<Func<TItem, bool>> predicate);

        #endregion

        #region Find

        public IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate, int? take = null, int skip = 0)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            return FindAsyncInternal(predicate, take, skip);
        }

        public IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            int? take = null,
            int skip = 0)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            Contract.Requires(orderBy != null);
            return FindAsyncInternal(predicate, orderBy, Order.By, take, skip);
        }

        public IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            int? take = null,
            int skip = 0)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            Contract.Requires(orderBy != null);
            return FindAsyncInternal(predicate, orderBy, orderType, take, skip);
        }

        public IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Expression<Func<TItem, object>> thenBy,
            int? take = null,
            int skip = 0)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            Contract.Requires(orderBy != null);
            Contract.Requires(thenBy != null);
            return FindAsyncInternal(predicate, orderBy, Order.By, thenBy, Order.By, take, skip);
        }

        public IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            Expression<Func<TItem, object>> thenBy,
            int? take = null,
            int skip = 0)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            Contract.Requires(orderBy != null);
            Contract.Requires(thenBy != null);
            return FindAsyncInternal(predicate, orderBy, orderType, thenBy, Order.By, take, skip);
        }

        public IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            Expression<Func<TItem, object>> thenBy,
            Order thenType,
            int? take = null,
            int skip = 0)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            Contract.Requires(orderBy != null);
            Contract.Requires(thenBy != null);
            return FindAsyncInternal(predicate, orderBy, orderType, thenBy, thenType, take, skip);
        }

        protected abstract IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate, int? take = null, int skip = 0);

        protected abstract IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            int? take = null,
            int skip = 0);

        protected abstract IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            Expression<Func<TItem, object>> thenBy,
            Order thenType,
            int? take = null,
            int skip = 0);

        #endregion

        #region Count

        public Task<uint> CountAsync()
        {
            Contract.Requires(IsInitialized);
            return CountAsyncInternal();
        }

        public Task<uint> CountAsync(Expression<Func<TItem, bool>> predicate)
        {
            Contract.Requires(IsInitialized);
            Contract.Requires(predicate != null);
            return CountAsyncInternal(predicate);
        }

        protected abstract Task<uint> CountAsyncInternal();

        protected abstract Task<uint> CountAsyncInternal(Expression<Func<TItem, bool>> predicate);

        #endregion
    }
}