using System;
using System.Linq.Expressions;

namespace ManagedCode.Database.Core
{
    public interface IOrderedCollectionQueryable<TSource> : ICollectionQueryable<TSource>
    {
        IOrderedCollectionQueryable<TSource> ThenBy(Expression<Func<TSource, object>> keySelector);
        IOrderedCollectionQueryable<TSource> ThenByDescending(Expression<Func<TSource, object>> keySelector);
    }
}