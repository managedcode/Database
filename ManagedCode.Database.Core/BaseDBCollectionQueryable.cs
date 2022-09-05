using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.Core;


public abstract class BaseDBCollectionQueryable<TSource> : IDBCollectionQueryable<TSource>
{
    protected List<Expression<Func<TSource, bool>>> WherePredicates = new ();
    protected List<Expression<Func<TSource, object>>> OrderByPredicates = new ();
    protected List<Expression<Func<TSource, object>> > OrderByDescendingPredicates = new ();
    protected int? TakeValue;
    protected int? SkipValue;

    public IDBCollectionQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate)
    {
        WherePredicates.Add(predicate);
        return this;
    }
    public IDBCollectionQueryable<TSource> OrderBy<TKey>(Expression<Func<TSource, object>> keySelector)
    {
        OrderByPredicates.Add(keySelector);
        return this;
    }

    public IDBCollectionQueryable<TSource> OrderByDescending<TKey>(Expression<Func<TSource, object>> keySelector)
    {
        OrderByDescendingPredicates.Add(keySelector);
        return this;
    }
    

    public IDBCollectionQueryable<TSource> Take(int count)
    {
        TakeValue = count;
        return this;
    }

    public IDBCollectionQueryable<TSource> Skip(int count)
    {
        SkipValue = count;
        return this;
    }

    public abstract IAsyncEnumerable<TSource> ToAsyncEnumerable(CancellationToken cancellationToken = default);

    public abstract Task<long> LongCountAsync(CancellationToken cancellationToken = default);

    public abstract Task<int> DeleteAsync(CancellationToken cancellationToken = default);
}