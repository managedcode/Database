using System;
using System.Linq.Expressions;

namespace ManagedCode.Database.Core;

public interface IDBCollectionQueryable<TSource> : IDBCollectionQueryableResultAsync<TSource>
{
    IDBCollectionQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate);
    IDBOrderedCollectionQueryable<TSource> OrderBy(Expression<Func<TSource, object>> keySelector);
    IDBOrderedCollectionQueryable<TSource> OrderByDescending(Expression<Func<TSource, object>> keySelector);
    IDBCollectionQueryable<TSource> Take(int? count);
    IDBCollectionQueryable<TSource> Skip(int count);
}