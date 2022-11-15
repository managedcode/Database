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

    private IQueryable<TItem> GetItemsInternal()
    {
        var cosmosQuery = _query;

        foreach (var query in Predicates)
        {
            switch (query.QueryType)
            {
                case QueryType.Where:
                    cosmosQuery = cosmosQuery.Where(x => query.ExpressionBool.Compile().Invoke(x));
                    break;

                case QueryType.OrderBy:
                    // TODO: Maybe need to check is IOrderedQueryable and do throw exception

                    cosmosQuery = cosmosQuery.OrderBy(x => query.ExpressionObject.Compile().Invoke(x));
                    break;

                case QueryType.OrderByDescending:
                    // TODO: Maybe need to check is IOrderedQueryable and do throw exception

                    cosmosQuery = cosmosQuery.OrderByDescending(x => query.ExpressionObject.Compile().Invoke(x));
                    break;

                case QueryType.ThenBy:
                    if (cosmosQuery is IOrderedQueryable<TItem> orderedItems)
                    {
                        cosmosQuery = orderedItems.ThenBy(x => query.ExpressionObject.Compile().Invoke(x));
                        break;
                    }
                    throw new InvalidOperationException("Before ThenBy call first OrderBy.");

                case QueryType.ThenByDescending:
                    if (cosmosQuery is IOrderedQueryable<TItem> orderedDescendingItems)
                    {
                        cosmosQuery = orderedDescendingItems.ThenBy(x => query.ExpressionObject.Compile().Invoke(x));
                        break;
                    }
                    throw new InvalidOperationException("Before ThenBy call first OrderBy.");

                case QueryType.Take:
                    if (query.Count.HasValue)
                    {
                        cosmosQuery = cosmosQuery.Take(query.Count.Value);
                    }
                    break;

                case QueryType.Skip:
                    if (query.Count.HasValue)
                    {
                        cosmosQuery = cosmosQuery.Skip(query.Count.Value);
                    }
                    break;

                default:
                    break;
            }
        }

        return cosmosQuery;
    }


    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var feedIterator = GetItemsInternal().ToFeedIterator();

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

        return GetItemsInternal().FirstOrDefault();
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return await GetItemsInternal().CountAsync(cancellationToken);
    }

    public override Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}