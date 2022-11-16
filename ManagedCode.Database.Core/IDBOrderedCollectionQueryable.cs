using System;
using System.Linq.Expressions;

namespace ManagedCode.Database.Core;

public interface IDBOrderedCollectionQueryable<TSource> : IDBCollectionQueryable<TSource>
{
    IDBOrderedCollectionQueryable<TSource> ThenBy(Expression<Func<TSource, object>> keySelector);
    IDBOrderedCollectionQueryable<TSource> ThenByDescending(Expression<Func<TSource, object>> keySelector);
}