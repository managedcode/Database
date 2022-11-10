using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos.Table;
using ManagedCode.Database.AzureTable.Extensions;


namespace ManagedCode.Database.AzureTable;

public class AzureTableDBCollectionQueryable<TItem> : OldBaseDBCollectionQueryable<TItem> where TItem : ITableEntity, new()
{
    private readonly AzureTableAdapter<TItem> _tableAdapter;

    public AzureTableDBCollectionQueryable(AzureTableAdapter<TItem> azureTableAdapter)
    {
        _tableAdapter = azureTableAdapter;
    }

    public override IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
    {
        var query = new TableQuery<TItem>();

        query = query
            .Where(WherePredicates)
            .CustomSelect(item => new DynamicTableEntity(item.PartitionKey, item.RowKey))
            .OrderBy(OrderByPredicates)
            .OrderByDescending(OrderByDescendingPredicates)
            .Take(TakeValue)
            .Skip(SkipValue);

        return _tableAdapter.ExecuteQuery(query, cancellationToken);
    }

    public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        var query = new TableQuery<TItem>();

        query = query
            .Where(WherePredicates)
            .CustomSelect(item => new DynamicTableEntity(item.PartitionKey, item.RowKey));

        return await _tableAdapter.ExecuteQuery(query, cancellationToken).FirstOrDefaultAsync(cancellationToken);
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var query = new TableQuery<TItem>();

        query = query
            .Where(WherePredicates)
            .CustomSelect(item => new DynamicTableEntity(item.PartitionKey, item.RowKey));

        return await _tableAdapter
            .ExecuteQuery(query, cancellationToken)
            .LongCountAsync(cancellationToken: cancellationToken);
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        int count, totalCount = 0;

        do
        {
            cancellationToken.ThrowIfCancellationRequested();

            var query = new TableQuery<TItem>();

            query = query
                .Where(WherePredicates)
                .CustomSelect(item => new DynamicTableEntity(item.PartitionKey, item.RowKey))
                .Take(_tableAdapter.BatchSize);

            var items = await _tableAdapter
                .ExecuteQuery(query, cancellationToken)
                .ToListAsync(cancellationToken: cancellationToken);

            count = items.Count;

            cancellationToken.ThrowIfCancellationRequested();
            totalCount += await _tableAdapter.ExecuteBatchAsync(items.Select(s =>
                TableOperation.Delete(new DynamicTableEntity(s.PartitionKey, s.RowKey)
                {
                    ETag = "*"
                })), cancellationToken);
        } while (count > 0);

        return totalCount;
    }
}