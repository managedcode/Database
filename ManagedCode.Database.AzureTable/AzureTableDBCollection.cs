using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable;

public class AzureTableDBCollection<TItem> : BaseDBCollection<TableId, TItem>
    where TItem : AzureTableItem, new()
{
    private readonly AzureTableAdapter<TItem> _tableAdapter;

    public AzureTableDBCollection(AzureTableAdapter<TItem> tableAdapter)
    {
        _tableAdapter = tableAdapter;
    }

    public override void Dispose()
    {
    }

    public override ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    #region Get

    protected override Task<TItem> GetAsyncInternal(TableId id, CancellationToken token = default)
    {
        return _tableAdapter.ExecuteAsync<TItem>(TableOperation.Retrieve<TItem>(id.PartitionKey, id.RowKey), token);
    }

    #endregion

    #region Count

    protected override async Task<long> CountAsyncInternal(CancellationToken token = default)
    {
        var count = 0;

        Expression<Func<TItem, bool>> predicate = item => true;

        await foreach (var item in _tableAdapter
                           .Query<DynamicTableEntity>(null, null, null,
                               item => new DynamicTableEntity(item.PartitionKey, item.RowKey),
                               cancellationToken: token))
        {
            count++;
        }

        return count;
    }

    #endregion

    public override IDBCollectionQueryable<TItem> Query()
    {
        return new AzureTableDBCollectionQueryable<TItem>(_tableAdapter);
    }

    #region Insert

    protected override async Task<TItem> InsertAsyncInternal(TItem item, CancellationToken token = default)
    {
        var result = await _tableAdapter.ExecuteAsync(TableOperation.Insert(item), token);
        return result as TItem;
    }

    protected override async Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        return await _tableAdapter.ExecuteBatchAsync(items.Select(s => TableOperation.Insert(s)), token);
    }

    #endregion

    #region InsertOrUpdate

    protected override async Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        var result = await _tableAdapter.ExecuteAsync<TItem>(TableOperation.InsertOrReplace(item), token);
        return result;
    }

    protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        return await _tableAdapter.ExecuteBatchAsync(items.Select(s => TableOperation.InsertOrReplace(s)), token);
    }

    #endregion

    #region Update

    protected override async Task<TItem> UpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(item.ETag))
        {
            item.ETag = "*";
        }

        var result = await _tableAdapter.ExecuteAsync<TItem>(TableOperation.Replace(item), token);
        return result;
    }

    protected override async Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        return await _tableAdapter.ExecuteBatchAsync(items.Select(s =>
        {
            if (string.IsNullOrEmpty(s.ETag))
            {
                s.ETag = "*";
            }

            return TableOperation.Replace(s);
        }), token);
    }

    #endregion

    #region Delete

    protected override async Task<bool> DeleteAsyncInternal(TableId id, CancellationToken token = default)
    {
        var result = await _tableAdapter.ExecuteAsync(TableOperation.Delete(new DynamicTableEntity(id.PartitionKey, id.RowKey)
        {
            ETag = "*"
        }), token);
        return result != null;
    }

    protected override async Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
    {
        var result = await _tableAdapter.ExecuteAsync(TableOperation.Delete(new DynamicTableEntity(item.PartitionKey, item.RowKey)
        {
            ETag = "*"
        }), token);
        return result != null;
    }

    protected override async Task<int> DeleteAsyncInternal(IEnumerable<TableId> ids, CancellationToken token = default)
    {
        return await _tableAdapter.ExecuteBatchAsync(ids.Select(s => TableOperation.Delete(new DynamicTableEntity(s.PartitionKey, s.RowKey)
        {
            ETag = "*"
        })), token);
    }

    protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        return await _tableAdapter.ExecuteBatchAsync(items.Select(s => TableOperation.Delete(new DynamicTableEntity(s.PartitionKey, s.RowKey)
        {
            ETag = "*"
        })), token);
    }

    protected override async Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
    {
        //return _tableAdapter.DropTable(token);
        return await Query().DeleteAsync(token) > 0;
    }

    #endregion
}