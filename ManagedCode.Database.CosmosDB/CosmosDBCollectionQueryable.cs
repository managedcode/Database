using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos.Linq;

namespace ManagedCode.Database.CosmosDB;

public class CosmosDBCollectionQueryable<TItem> : BaseDBCollectionQueryable<TItem> where TItem : CosmosDbItem, IItem<string>, new()
{
    private IQueryable<TItem> _query;

    public CosmosDBCollectionQueryable(IQueryable<TItem> query)
    {
        _query = query;
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
    {
        foreach (var predicate in WherePredicates)
        {
            _query = _query.Where(predicate);
        }

        if (OrderByPredicates.Count > 0)
        {
            IOrderedQueryable<TItem> ordered = null;
            var firstOrderBy = true;
            foreach (var predicate in OrderByPredicates)
            {
                if (firstOrderBy)
                {
                    ordered = _query.OrderBy(predicate);
                    firstOrderBy = false;
                }
                else
                {
                    ordered = ordered.ThenBy(predicate);
                }
            }

            _query = ordered;
        }

        if (OrderByDescendingPredicates.Count > 0)
        {
            IOrderedQueryable<TItem> ordered = null;
            var firstOrderByDescending = true;
            foreach (var predicate in OrderByDescendingPredicates)
            {
                if (firstOrderByDescending)
                {
                    ordered = _query.OrderByDescending(predicate);
                    firstOrderByDescending = false;
                }
                else
                {
                    ordered = ordered.ThenByDescending(predicate);
                }
            }

            _query = ordered;
        }

        if (SkipValue.HasValue)
        {
            _query = _query.Skip(SkipValue.Value);
        }

        if (TakeValue.HasValue)
        {
            _query = _query.Take(TakeValue.Value);
        }

        var feedIterator = _query.ToFeedIterator();
        using (var iterator = feedIterator)
        {
            while (iterator.HasMoreResults)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var item in await iterator.ReadNextAsync(cancellationToken))
                {
                    yield return item;
                }
            }
        }
    }

    public override async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
    {
        foreach (var predicate in WherePredicates)
        {
            _query = _query.Where(predicate);
        }

        return await _query.CountAsync(cancellationToken);
    }

    public override Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}