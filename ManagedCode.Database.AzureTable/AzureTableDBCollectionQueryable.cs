using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using ManagedCode.Database.Core;


namespace ManagedCode.Database.AzureTable;

public class AzureTableDBCollectionQueryable<TItem> : BaseDBCollectionQueryable<TItem>
    where TItem : class, ITableEntity, new()
{
    private readonly TableClient _tableClient;

    public AzureTableDBCollectionQueryable(TableClient tableClient)
    {
        _tableClient = tableClient;
    }

    public override IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
    {
        var filter = ConvertPredicatesToFilter(Predicates);
        var query = _tableClient.QueryAsync<TItem>(filter, cancellationToken: cancellationToken);

        return ApplyPredicates(query, Predicates);
    }

    public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        var filter = ConvertPredicatesToFilter(Predicates);

        return await _tableClient
            .QueryAsync<TItem>(filter, maxPerPage: 1, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var filter = ConvertPredicatesToFilter(Predicates);

        return await _tableClient
            .QueryAsync<TItem>(filter, cancellationToken: cancellationToken)
            .CountAsync(cancellationToken);
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        var filter = ConvertPredicatesToFilter(Predicates);

        var items = await _tableClient
            .QueryAsync<TItem>(filter, cancellationToken: cancellationToken).ToListAsync(cancellationToken);

        var actions = items
            .Select(item =>
                _tableClient.DeleteEntityAsync(item.PartitionKey, item.RowKey, ETag.All, cancellationToken));

        return await BatchHelper.ExecuteAsync(actions, token: cancellationToken);
    }

    private static IAsyncEnumerable<TItem> ApplyPredicates(IAsyncEnumerable<TItem> asyncEnumerable, List<QueryItem> predicates)
    {
        foreach (var query in predicates)
        {
            switch (query.QueryType)
            {
                case QueryType.OrderBy:
                    asyncEnumerable = asyncEnumerable.OrderBy(x => query.ExpressionObject.Compile().Invoke(x));
                    break;
                case QueryType.OrderByDescending:
                    asyncEnumerable =
                        asyncEnumerable.OrderByDescending(x => query.ExpressionObject.Compile().Invoke(x));
                    break;
                case QueryType.ThenBy:
                {
                    if (asyncEnumerable is IOrderedAsyncEnumerable<TItem> orderedEnumerable)
                    {
                        asyncEnumerable = orderedEnumerable.ThenBy(x => query.ExpressionObject.Compile().Invoke(x));
                    }

                    // TODO: Maybe need throw exception
                    break;
                }
                case QueryType.ThenByDescending:
                {
                    if (asyncEnumerable is IOrderedAsyncEnumerable<TItem> orderedDescendingEnumerable)
                    {
                        asyncEnumerable =
                            orderedDescendingEnumerable.ThenByDescending(
                                x => query.ExpressionObject.Compile().Invoke(x));
                    }

                    // TODO: Maybe need throw exception
                    break;
                }
                case QueryType.Take:
                    if (query.Count.HasValue)
                    {
                        asyncEnumerable = asyncEnumerable.Take(query.Count.Value);
                    }
                    break;
                case QueryType.Skip:
                    if (query.Count.HasValue)
                    {
                        asyncEnumerable = asyncEnumerable.Skip(query.Count.Value);
                    }
                    break;
            }
        }

        return asyncEnumerable;
    }

    private static string ConvertPredicatesToFilter(IEnumerable<QueryItem> predicates)
    {
        var filter = predicates
            .Where(p => p.QueryType is QueryType.Where)
            .Select(p => TableClient.CreateQueryFilter(p.ExpressionBool))
            .Aggregate((a, b) => a + " and " + b);

        return filter;
    }
}