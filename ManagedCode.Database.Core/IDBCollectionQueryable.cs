using System;
using System.Linq.Expressions;

namespace ManagedCode.Database.Core.Queries;

public interface IDBCollectionQueryable<TSource> : IDBCollectionQueryableResultAsync<TSource>
{
    IDBCollectionQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate);
    IDBCollectionQueryable<TSource> OrderBy<TKey>(Expression<Func<TSource, object>> keySelector);
    IDBCollectionQueryable<TSource> OrderByDescending<TKey>(Expression<Func<TSource, object>> keySelector);
    IDBCollectionQueryable<TSource> Take(int count);
    IDBCollectionQueryable<TSource> Skip(int count);
}