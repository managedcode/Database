using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.Core;

public abstract class BaseDBCollectionQueryable<TSource> : IDBCollectionQueryable<TSource>
{
    protected int? TakeValue;
    protected int? SkipValue;
    protected readonly List<Expression<Func<TSource, bool>>> WherePredicates = new();
    protected readonly List<Expression<Func<TSource, object>>> OrderByPredicates = new();
    protected readonly List<Expression<Func<TSource, object>>> OrderByDescendingPredicates = new();

    public abstract IAsyncEnumerable<TSource> ToAsyncEnumerable(CancellationToken cancellationToken = default);
    public abstract Task<TSource?> FirstOrDefaultAsync(CancellationToken cancellationToken = default);
    public abstract Task<long> CountAsync(CancellationToken cancellationToken = default);
    public abstract Task<int> DeleteAsync(CancellationToken cancellationToken = default);

    public IDBCollectionQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate)
    {
        WherePredicates.Add(predicate);
        return this;
    }

    public IDBCollectionQueryable<TSource> OrderBy(Expression<Func<TSource, object>> keySelector)
    {
        OrderByPredicates.Add(keySelector);
        return this;
    }

    public IDBCollectionQueryable<TSource> OrderByDescending(Expression<Func<TSource, object>> keySelector)
    {
        OrderByDescendingPredicates.Add(keySelector);
        return this;
    }

    public IDBCollectionQueryable<TSource> Take(int? count)
    {
        TakeValue = count;
        return this;
    }

    public IDBCollectionQueryable<TSource> Skip(int count)
    {
        SkipValue = count;
        return this;
    }

    public async Task<List<TSource>> ToListAsync(CancellationToken cancellationToken = default)
    {
        return new List<TSource>(await ToAsyncEnumerable(cancellationToken).ToListAsync(cancellationToken));
    }
}