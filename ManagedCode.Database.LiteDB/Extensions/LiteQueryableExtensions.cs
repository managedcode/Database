using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LiteDB;

namespace ManagedCode.Database.LiteDB.Extensions;

internal static class LiteQueryableExtensions
{
    internal static ILiteQueryable<TItem> Where<TItem>(this ILiteQueryable<TItem> query,
        List<Expression<Func<TItem, bool>>> predicates)
    {
        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        return query;
    }

    internal static ILiteQueryable<TItem> OrderBy<TItem>(this ILiteQueryable<TItem> query,
        List<Expression<Func<TItem, object>>> predicates)
    {
        if (predicates.Count > 0)
        {
            foreach (var predicate in predicates)
            {
                query = query.OrderBy(predicate);
            }
        }

        return query;
    }

    internal static ILiteQueryable<TItem> OrderByDescending<TItem>(this ILiteQueryable<TItem> query,
        List<Expression<Func<TItem, object>>> predicates)
    {
        if (predicates.Count > 0)
        {
            foreach (var predicate in predicates)
            {
                query = query.OrderByDescending(predicate);
            }
        }

        return query;
    }

    internal static ILiteQueryableResult<TItem> Skip<TItem>(this ILiteQueryable<TItem> query, int? skip)
    {
        skip ??= 0;

        return query.Skip(skip.Value);
    }

    internal static ILiteQueryableResult<TItem> Take<TItem>(this ILiteQueryableResult<TItem> query, int? take)
    {
        take ??= int.MaxValue;

        return query.Limit(take.Value);
    }
}