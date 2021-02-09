using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ManagedCode.Repository.Core
{
    [ContractClass(typeof(BaseRepository<,>))]
    public interface IRepository<in TId, TItem> where TItem : IRepositoryItem<TId>
    {
        bool IsInitialized { get; }

        Task InitializeAsync();

        Task<bool> InsertAsync(TItem item);
        Task<int> InsertAsync(IEnumerable<TItem> items);

        Task<bool> UpdateAsync(TItem item);
        Task<int> UpdateAsync(IEnumerable<TItem> items);

        Task<bool> InsertOrUpdateAsync(TItem item);
        Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items);

        Task<bool> DeleteAsync(TId id);
        Task<bool> DeleteAsync(TItem item);
        Task<int> DeleteAsync(IEnumerable<TId> ids);
        Task<int> DeleteAsync(IEnumerable<TItem> items);
        Task<int> DeleteAsync(Expression<Func<TItem, bool>> predicate);
        Task<bool> DeleteAllAsync();

        Task<TItem> GetAsync(TId id);
        Task<TItem> GetAsync(Expression<Func<TItem, bool>> predicate);

        IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate, int? take = null, int skip = 0);

        IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            int? take = null,
            int skip = 0);

        IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            int? take = null,
            int skip = 0);

        IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Expression<Func<TItem, object>> thenBy,
            int? take = null,
            int skip = 0);

        IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            Expression<Func<TItem, object>> thenBy,
            int? take = null,
            int skip = 0);

        IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            Expression<Func<TItem, object>> thenBy,
            Order thenType,
            int? take = null,
            int skip = 0);

        Task<uint> CountAsync();
        Task<uint> CountAsync(Expression<Func<TItem, bool>> predicate);
    }
}