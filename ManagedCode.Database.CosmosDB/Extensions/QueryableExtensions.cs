using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ManagedCode.Database.CosmosDB.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<TItem> OrderBy<TItem>(this IQueryable<TItem> query,
        List<Expression<Func<TItem, object>>> predicates)
    {
        IOrderedQueryable<TItem>? ordered = null;

        if (predicates.Count > 0)
        {
            foreach (var predicate in predicates)
            {
                ordered = ordered switch
                {
                    null => query.OrderBy(predicate),
                    _ => ordered.ThenBy(predicate)
                };
            }
        }

        return ordered ?? query;
    }

    public static IQueryable<TItem> OrderByDescending<TItem>(this IQueryable<TItem> query,
        List<Expression<Func<TItem, object>>> predicates)
    {
        IOrderedQueryable<TItem>? ordered = null;

        if (predicates.Count > 0)
        {
            foreach (var predicate in predicates)
            {
                ordered = ordered switch
                {
                    null => query.OrderByDescending(predicate),
                    _ => ordered.ThenByDescending(predicate)
                };
            }
        }

        return ordered ?? query;
    }

    public static IQueryable<TItem> Skip<TItem>(this IQueryable<TItem> query, int? skip)
    {
        if (skip.HasValue)
        {
            query = Queryable.Skip(query, skip.Value);
        }

        return query;
    }

    public static IQueryable<TItem> Take<TItem>(this IQueryable<TItem> query, int? take)
    {
        if (take.HasValue)
        {
            query = Queryable.Take(query, take.Value);
        }

        return query;
    }
}