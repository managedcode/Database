using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        var filter = CreateFiler();

        return GetItemsInternal(_tableClient.QueryAsync<TItem>(filter, cancellationToken: cancellationToken)
            .AsAsyncEnumerable());
    }

    public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        var filter = CreateFiler();

        return await _tableClient
            .QueryAsync<TItem>(filter, maxPerPage: 1, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var filter = CreateFiler();

        return await _tableClient
            .QueryAsync<TItem>(filter, cancellationToken: cancellationToken)
            .CountAsync(cancellationToken);
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        // int count, totalCount = 0;
        //
        // do
        // {
        //     cancellationToken.ThrowIfCancellationRequested();
        //
        //     var filter = CreateFiler();
        //
        //     var items = await _tableClient
        //         .QueryAsync<TItem>(filter, cancellationToken: cancellationToken)
        //         .ToListAsync(cancellationToken);
        //
        //
        //     count = items.Count;
        //
        //     cancellationToken.ThrowIfCancellationRequested();
        //     totalCount += await _tableClient.DeleteEntityAsync() .ExecuteBatchAsync(items.Select(s =>
        //         TableOperation.Delete(new DynamicTableEntity(s.PartitionKey, s.RowKey)
        //         {
        //             ETag = "*"
        //         })), cancellationToken);
        // } while (count > 0);
        //
        // // return totalCount;
        //
        throw new NotImplementedException();
    }

    private IAsyncEnumerable<TItem> GetItemsInternal(IAsyncEnumerable<TItem> asyncEnumerable)
    {
        foreach (var query in Predicates)
        {
            if (query.QueryType == QueryType.OrderBy)
            {
                asyncEnumerable = asyncEnumerable.OrderBy(x => query.ExpressionObject.Compile().Invoke(x));
            }
            else if (query.QueryType == QueryType.OrderByDescending)
            {
                asyncEnumerable =
                    asyncEnumerable.OrderByDescending(x => query.ExpressionObject.Compile().Invoke(x));
            }
            else if (query.QueryType == QueryType.ThenBy)
            {
                if (asyncEnumerable is IOrderedAsyncEnumerable<TItem> orderedEnumerable)
                {
                    asyncEnumerable = orderedEnumerable.ThenBy(x => query.ExpressionObject.Compile().Invoke(x));
                }
            }
            else if (query.QueryType == QueryType.ThenByDescending)
            {
                if (asyncEnumerable is IOrderedAsyncEnumerable<TItem> orderedDescendingEnumerable)
                {
                    asyncEnumerable =
                        orderedDescendingEnumerable.ThenByDescending(
                            x => query.ExpressionObject.Compile().Invoke(x));
                }
            }
            else if (query.QueryType == QueryType.Take)
            {
                asyncEnumerable = asyncEnumerable.Take(query.Count.GetValueOrDefault());
            }
            else if (query.QueryType == QueryType.Skip)
            {
                asyncEnumerable = asyncEnumerable.Skip(query.Count.GetValueOrDefault());
            }
        }

        return asyncEnumerable;
    }

    private string CreateFiler()
    {
        var filter = Predicates
            .Where(p => p.QueryType is QueryType.Where)
            .Select(p => TableClient.CreateQueryFilter(p.ExpressionBool))
            .Aggregate((a, b) => a + " and " + b);

        return filter;
    }
}