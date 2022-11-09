using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos.Linq;
using ManagedCode.Database.CosmosDB.Extensions;

namespace ManagedCode.Database.CosmosDB;

public class CosmosDBCollectionQueryable<TItem> : BaseDBCollectionQueryable<TItem>
    where TItem : CosmosDBItem, IItem<string>, new()
{
    private readonly IQueryable<TItem> _query;

    public CosmosDBCollectionQueryable(IQueryable<TItem> query)
    {
        _query = query;
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var feedIterator = _query
            .Where(WherePredicates)
            .OrderBy(OrderByPredicates)
            .OrderByDescending(OrderByDescendingPredicates)
            .Take(TakeValue)
            .Skip(SkipValue)
            .ToFeedIterator();

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

    public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        return _query
            .Where(WherePredicates)
            .FirstOrDefault();
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _query
            .Where(WherePredicates)
            .CountAsync(cancellationToken);
    }

    public override Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}