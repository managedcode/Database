using System.Collections.Generic;
using ManagedCode.Database.Core;
using MongoDB.Driver.Linq;

namespace ManagedCode.Database.MongoDB.Extensions;

internal static class MongoQueryableExtensions
{
    internal static IMongoQueryable<TItem> ApplyPredicates<TItem>(this IMongoQueryable<TItem> query,
        IEnumerable<BaseDBCollectionQueryable<TItem>.QueryItem> predicates)
    {
        foreach (var predicate in predicates)
        {
            switch (predicate.QueryType)
            {
                case BaseDBCollectionQueryable<TItem>.QueryType.OrderBy:
                    query = query.OrderBy(x => predicate.ExpressionObject.Compile().Invoke(x));
                    break;

                case BaseDBCollectionQueryable<TItem>.QueryType.OrderByDescending:
                    query = query.OrderByDescending(x => predicate.ExpressionObject.Compile().Invoke(x));
                    break;

                case BaseDBCollectionQueryable<TItem>.QueryType.ThenBy:
                    query = (query as IOrderedMongoQueryable<TItem>)
                        .ThenBy(x => predicate.ExpressionObject.Compile().Invoke(x));
                    break;

                case BaseDBCollectionQueryable<TItem>.QueryType.ThenByDescending:
                    query = (query as IOrderedMongoQueryable<TItem>)
                        .ThenByDescending(x => predicate.ExpressionObject.Compile().Invoke(x));
                    break;

                case BaseDBCollectionQueryable<TItem>.QueryType.Take:
                    if (predicate.Count.HasValue)
                    {
                        query = query.Take(predicate.Count.Value);
                    }

                    break;

                case BaseDBCollectionQueryable<TItem>.QueryType.Skip:
                    query = query.Skip(predicate.Count!.Value);
                    break;
            }
        }

        return query;
    }
}