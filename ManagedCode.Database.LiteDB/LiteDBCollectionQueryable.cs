using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using ManagedCode.Database.Core;

namespace ManagedCode.Database.LiteDB;

public class LiteDBCollectionQueryable<TId, TItem> : BaseCollectionQueryable<TItem>
    where TItem : LiteDBItem<TId>, IItem<TId>, new()
{
    private readonly ILiteCollection<TItem> _collection;

    public LiteDBCollectionQueryable(ILiteCollection<TItem> collection)
    {
        _collection = collection;
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        foreach (var item in ApplyPredicates(Predicates).ToEnumerable())
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }
    }

    public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        var query = ApplyPredicates(Predicates);

        return await Task.Run(() => query.FirstOrDefault(), cancellationToken);
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var query = ApplyPredicates(Predicates);

        return await Task.Run(() => query.LongCount(), cancellationToken);
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        var wherePredicates = Predicates
            .Where(p => p.QueryType is QueryType.Where)
            .ToList();

        if (!wherePredicates.Any())
        {
            return await Task.Run(() => _collection.DeleteAll(), cancellationToken);
        }

        var predicates = Predicates
            .Select(p => p.ExpressionBool)
            .Aggregate(CombineExpressions);

        return await Task.Run(() => _collection.DeleteMany(predicates), cancellationToken);
    }

    private ILiteQueryableResult<TItem> ApplyPredicates(IEnumerable<QueryItem> predicates)
    {
        // TODO: optimize
        var query = _collection.Query();

        foreach (var predicate in predicates)
            query = predicate.QueryType switch
            {
                QueryType.Where => query.Where(predicate.ExpressionBool),
                QueryType.OrderBy => query.OrderBy(predicate.ExpressionObject),
                QueryType.OrderByDescending => query.OrderByDescending(predicate.ExpressionObject),
                QueryType.ThenBy => query.OrderBy(predicate.ExpressionObject),
                QueryType.ThenByDescending => query.OrderByDescending(predicate.ExpressionObject),
                _ => query
            };

        ILiteQueryableResult<TItem> queryableResult = query;

        foreach (var predicate in predicates)
            queryableResult = predicate.QueryType switch
            {
                QueryType.Take => predicate.Count.HasValue
                    ? queryableResult.Limit(predicate.Count.Value)
                    : queryableResult,

                QueryType.Skip => queryableResult.Skip(predicate.Count!.Value),
                _ => queryableResult
            };

        return queryableResult;
    }

    private static Expression<Func<T, bool>> CombineExpressions<T>(Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var body = Expression.AndAlso(expr1.Body, expr2.Body);
        return Expression.Lambda<Func<T, bool>>(body, expr1.Parameters[0]);
    }
}