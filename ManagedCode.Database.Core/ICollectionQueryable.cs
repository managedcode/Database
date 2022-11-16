using System;
using System.Linq.Expressions;

namespace ManagedCode.Database.Core
{
    public interface ICollectionQueryable<TSource> : ICollectionQueryableResultAsync<TSource>
    {
        ICollectionQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate);
        IOrderedCollectionQueryable<TSource> OrderBy(Expression<Func<TSource, object>> keySelector);
        IOrderedCollectionQueryable<TSource> OrderByDescending(Expression<Func<TSource, object>> keySelector);
        ICollectionQueryable<TSource> Take(int? count);
        ICollectionQueryable<TSource> Skip(int count);
    }
}