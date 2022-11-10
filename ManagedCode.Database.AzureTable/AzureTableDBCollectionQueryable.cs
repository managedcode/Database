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
        // _tableClient.Query<TItem>();
        // var query = _tableClient.Query();
        //
        // query = query
        //     .Where(WherePredicates)
        //     .OrderBy(OrderByPredicates)
        //     .OrderByDescending(OrderByDescendingPredicates)
        //     .Take(TakeValue)
        //     .Skip(SkipValue);
        //
        // return _tableClient.ExecuteQuery(query, cancellationToken);

        throw new NotImplementedException();
    }

    public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        // var query = new TableQuery<TItem>().Where(WherePredicates);
        //
        // return await _tableClient.ExecuteQuery(query, cancellationToken).FirstOrDefaultAsync(cancellationToken);

        throw new NotImplementedException();
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        // var query = new TableQuery<TItem>();
        //
        // query = query
        //     .Where(WherePredicates);
        // // .CustomSelect(item => new DynamicTableEntity(item.PartitionKey, item.RowKey));
        //
        // return await _tableClient
        //     .ExecuteQuery(query, cancellationToken)
        //     .LongCountAsync(cancellationToken: cancellationToken);
        throw new NotImplementedException();
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        // int count, totalCount = 0;
        //
        // do
        // {
        //     cancellationToken.ThrowIfCancellationRequested();
        //
        //     var query = new TableQuery<TItem>();
        //
        //     query = query
        //         .Where(WherePredicates)
        //         // .CustomSelect(item => new DynamicTableEntity(item.PartitionKey, item.RowKey))
        //         // TODO: check
        //         .Take(100);
        //
        //     var items = await _tableClient
        //         .ExecuteQuery(query, cancellationToken)
        //         .ToListAsync(cancellationToken: cancellationToken);
        //
        //     count = items.Count;
        //
        //     cancellationToken.ThrowIfCancellationRequested();
        //     totalCount += await _tableClient.ExecuteBatchAsync(items.Select(s =>
        //         TableOperation.Delete(new DynamicTableEntity(s.PartitionKey, s.RowKey)
        //         {
        //             ETag = "*"
        //         })), cancellationToken);
        // } while (count > 0);
        //
        // return totalCount;

        throw new NotImplementedException();
    }
}