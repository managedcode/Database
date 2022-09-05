using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.Core;

public interface IDBCollection<in TId, TItem> : IDisposable, IAsyncDisposable where TItem : IItem<TId>
{
    Task<TItem> InsertAsync(TItem item, CancellationToken token = default);
    Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken token = default);

    Task<TItem> UpdateAsync(TItem item, CancellationToken token = default);
    Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken token = default);

    Task<TItem> InsertOrUpdateAsync(TItem item, CancellationToken token = default);
    Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items, CancellationToken token = default);

    Task<bool> DeleteAsync(TId id, CancellationToken token = default);
    Task<bool> DeleteAsync(TItem item, CancellationToken token = default);
    Task<int> DeleteAsync(IEnumerable<TId> ids, CancellationToken token = default);
    Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken token = default);
    Task<int> DeleteAsync(Expression<Func<TItem, bool>> predicate, CancellationToken token = default);
    
    Task<bool> DeleteAllAsync(CancellationToken token = default);

    Task<TItem> GetAsync(TId id, CancellationToken token = default);
    Task<TItem> GetAsync(Expression<Func<TItem, bool>> predicate, CancellationToken token = default);
    
    IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
        int? take = null,
        int skip = 0,
        CancellationToken token = default);

    IAsyncEnumerable<TItem> FindAsync(IEnumerable<Expression<Func<TItem, bool>>> predicates,
        int? take = null,
        int skip = 0,
        CancellationToken token = default);

    IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
        Expression<Func<TItem, object>> orderBy,
        int? take = null,
        int skip = 0,
        CancellationToken token = default);

    IAsyncEnumerable<TItem> FindAsync(IEnumerable<Expression<Func<TItem, bool>>> predicates,
        Expression<Func<TItem, object>> orderBy,
        int? take = null,
        int skip = 0,
        CancellationToken token = default);

    IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
        Expression<Func<TItem, object>> orderBy,
        Order orderType,
        int? take = null,
        int skip = 0,
        CancellationToken token = default);

    IAsyncEnumerable<TItem> FindAsync(IEnumerable<Expression<Func<TItem, bool>>> predicates,
        Expression<Func<TItem, object>> orderBy,
        Order orderType,
        int? take = null,
        int skip = 0,
        CancellationToken token = default);

    IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
        Expression<Func<TItem, object>> orderBy,
        Expression<Func<TItem, object>> thenBy,
        int? take = null,
        int skip = 0,
        CancellationToken token = default);

    IAsyncEnumerable<TItem> FindAsync(IEnumerable<Expression<Func<TItem, bool>>> predicates,
        Expression<Func<TItem, object>> orderBy,
        Expression<Func<TItem, object>> thenBy,
        int? take = null,
        int skip = 0,
        CancellationToken token = default);

    IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
        Expression<Func<TItem, object>> orderBy,
        Order orderType,
        Expression<Func<TItem, object>> thenBy,
        int? take = null,
        int skip = 0,
        CancellationToken token = default);

    IAsyncEnumerable<TItem> FindAsync(Expression<Func<TItem, bool>> predicate,
        Expression<Func<TItem, object>> orderBy,
        Order orderType,
        Expression<Func<TItem, object>> thenBy,
        Order thenType,
        int? take = null,
        int skip = 0,
        CancellationToken token = default);

    IAsyncEnumerable<TItem> FindAsync(IEnumerable<Expression<Func<TItem, bool>>> predicates,
        Expression<Func<TItem, object>> orderBy,
        Order orderType,
        Expression<Func<TItem, object>> thenBy,
        Order thenType,
        int? take = null,
        int skip = 0,
        CancellationToken token = default);

    Task<long> CountAsync(CancellationToken token = default);
    Task<long> CountAsync(Expression<Func<TItem, bool>> predicate, CancellationToken token = default);
    Task<long> CountAsync(IEnumerable<Expression<Func<TItem, bool>>> predicates, CancellationToken token = default);
}