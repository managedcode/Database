using System;
using System.Linq.Expressions;

namespace ManagedCode.Database.Core;

public interface IDBCollectionQueryable<TSource> : IDBCollectionQueryableResultAsync<TSource>
{
    IDBCollectionQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate);
    IDBCollectionQueryable<TSource> OrderBy(Expression<Func<TSource, object>> keySelector);
    IDBCollectionQueryable<TSource> OrderByDescending(Expression<Func<TSource, object>> keySelector);/*
    IDBCollectionQueryable<TSource> ThenBy(Expression<Func<TSource, object>> keySelector);
    IDBCollectionQueryable<TSource> ThenByDescending(Expression<Func<TSource, object>> keySelector);*/
    IDBCollectionQueryable<TSource> Take(int? count);
    IDBCollectionQueryable<TSource> Skip(int count);
}