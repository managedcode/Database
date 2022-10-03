using System;
using System.Linq.Expressions;

namespace ManagedCode.Database.Core;

public interface IDBCollectionQueryable<TSource> : IDBCollectionQueryableResultAsync<TSource>
{
    IDBCollectionQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate);
    IDBCollectionQueryable<TSource> OrderBy(Expression<Func<TSource, object>> keySelector);
    IDBCollectionQueryable<TSource> OrderBy(Expression<Func<TSource, object>> keySelectorF, Expression<Func<TSource, object>> keySelectorS);
    IDBCollectionQueryable<TSource> OrderByDescending(Expression<Func<TSource, object>> keySelector);
    IDBCollectionQueryable<TSource> OrderByDescending(Expression<Func<TSource, object>> keySelectorF, Expression<Func<TSource, object>> keySelectorS);
    IDBCollectionQueryable<TSource> Take(int count);
    IDBCollectionQueryable<TSource> Skip(int count);
}