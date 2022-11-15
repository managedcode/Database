using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using ManagedCode.Database.Core;
using ManagedCode.Database.AzureTable.Extensions;

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

        var responses = await _tableClient.SubmitTransactionByChunksAsync<TItem>(items,
            TableTransactionActionType.Delete, cancellationToken);

        return responses.Count(v => !v.IsError);
    }

    private static IAsyncEnumerable<TItem> ApplyPredicates(IAsyncEnumerable<TItem> asyncEnumerable,
        List<QueryItem> predicates)
    {
        // TODO: add warning
        foreach (var predicate in predicates)
        {
            switch (predicate.QueryType)
            {
                case QueryType.OrderBy:
                    asyncEnumerable = asyncEnumerable.OrderBy(x => predicate.ExpressionObject.Compile().Invoke(x));
                    break;

                case QueryType.OrderByDescending:
                    asyncEnumerable =
                        asyncEnumerable.OrderByDescending(x => predicate.ExpressionObject.Compile().Invoke(x));
                    break;

                case QueryType.ThenBy:
                    asyncEnumerable = (asyncEnumerable as IOrderedAsyncEnumerable<TItem>)!
                        .ThenBy(x => predicate.ExpressionObject.Compile().Invoke(x));
                    break;

                case QueryType.ThenByDescending:
                    asyncEnumerable = (asyncEnumerable as IOrderedAsyncEnumerable<TItem>)!
                        .ThenByDescending(x => predicate.ExpressionObject.Compile().Invoke(x));
                    break;

                case QueryType.Take:
                    if (predicate.Count.HasValue)
                    {
                        asyncEnumerable = asyncEnumerable.Take(predicate.Count.Value);
                    }

                    break;

                case QueryType.Skip:
                    asyncEnumerable = asyncEnumerable.Skip(predicate.Count!.Value);
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