using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable;

public class AzureTableDBCollectionQueryable<TItem> : BaseDBCollectionQueryable<TItem> where TItem : ITableEntity, new()
{
    private readonly AzureTableAdapter<TItem> _tableAdapter;

    public AzureTableDBCollectionQueryable(AzureTableAdapter<TItem> azureTableAdapter)
    {
        _tableAdapter = azureTableAdapter;
    }

    public override IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
    {
        return _tableAdapter.Query(WherePredicates, OrderByPredicates, OrderByDescendingPredicates, null, TakeValue,
            SkipValue, cancellationToken);
    }

    public override async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
    {
        long count = 0;

        await foreach (var item in _tableAdapter.Query(WherePredicates, OrderByPredicates, OrderByDescendingPredicates,
                           item => new DynamicTableEntity(item.PartitionKey, item.RowKey),
                           TakeValue, SkipValue, cancellationToken))
        {
            count++;
        }

        return count;
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        var count = 0;
        var totalCount = 0;

        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            var ids = await _tableAdapter
                .Query(WherePredicates, null, null, item => new DynamicTableEntity(item.PartitionKey, item.RowKey),
                    _tableAdapter.BatchSize, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);

            count = ids.Count;

            cancellationToken.ThrowIfCancellationRequested();
            totalCount += await _tableAdapter.ExecuteBatchAsync(ids.Select(s => TableOperation.Delete(new DynamicTableEntity(s.PartitionKey, s.RowKey)
            {
                ETag = "*"
            })), cancellationToken);
        } while (count > 0);

        return totalCount;
    }
    
}