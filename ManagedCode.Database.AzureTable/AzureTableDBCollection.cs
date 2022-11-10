using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.AzureTable.Extensions;
using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable;

public class AzureTableDBCollection<TItem> : IDBCollection<TableId, TItem>
    where TItem : AzureTableItem, new()
{
    private readonly CloudTable _cloudTable;

    public AzureTableDBCollection(CloudTable cloudTable)
    {
        _cloudTable = cloudTable;
    }

    public IDBCollectionQueryable<TItem> Query => new AzureTableDBCollectionQueryable<TItem>(_cloudTable);

    #region Get

    public Task<TItem?> GetAsync(TableId id, CancellationToken token = default)
    {
        return _cloudTable.ExecuteAsync<TItem>(TableOperation.Retrieve<TItem>(id.PartitionKey, id.RowKey), token);
    }

    #endregion

    #region Count

    public async Task<long> CountAsync(CancellationToken token = default)
    {
        var query = new TableQuery<DynamicTableEntity>();
        query = query.CustomSelect(item => new DynamicTableEntity(item.PartitionKey, item.RowKey));

        return await _cloudTable.ExecuteQuery(query, cancellationToken: token).CountAsync(token);
    }

    #endregion

    #region Insert

    public async Task<TItem?> InsertAsync(TItem item, CancellationToken token = default)
    {
        var result = await _cloudTable.ExecuteAsync(TableOperation.Insert(item), token);
        return result as TItem;
    }

    public async Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        return await _cloudTable.ExecuteBatchAsync(items.Select(s => TableOperation.Insert(s)), token);
    }

    #endregion

    #region InsertOrUpdate

    public async Task<TItem?> InsertOrUpdateAsync(TItem item, CancellationToken token = default)
    {
        var result = await _cloudTable.ExecuteAsync<TItem>(TableOperation.InsertOrReplace(item), token);
        return result;
    }

    public async Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items,
        CancellationToken token = default)
    {
        return await _cloudTable.ExecuteBatchAsync(items.Select(TableOperation.InsertOrReplace), token);
    }

    #endregion

    #region Update

    public async Task<TItem?> UpdateAsync(TItem item, CancellationToken token = default)
    {
        if (string.IsNullOrEmpty(item.ETag))
        {
            item.ETag = "*";
        }

        var result = await _cloudTable.ExecuteAsync<TItem>(TableOperation.Replace(item), token);
        return result;
    }

    public async Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        return await _cloudTable.ExecuteBatchAsync(items.Select(s =>
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

    public async Task<bool> DeleteAsync(TableId id, CancellationToken token = default)
    {
        var result = await _cloudTable.ExecuteAsync(TableOperation.Delete(
            new DynamicTableEntity(id.PartitionKey, id.RowKey)
            {
                ETag = "*"
            }), token);
        return result is not null;
    }

    public async Task<bool> DeleteAsync(TItem item, CancellationToken token = default)
    {
        var result = await _cloudTable.ExecuteAsync(TableOperation.Delete(
            new DynamicTableEntity(item.PartitionKey, item.RowKey)
            {
                ETag = "*"
            }), token);
        return result is not null;
    }

    public async Task<int> DeleteAsync(IEnumerable<TableId> ids, CancellationToken token = default)
    {
        return await _cloudTable.ExecuteBatchAsync(ids.Select(s => TableOperation.Delete(
            new DynamicTableEntity(s.PartitionKey, s.RowKey)
            {
                ETag = "*"
            })), token);
    }

    public async Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        return await _cloudTable.ExecuteBatchAsync(items.Select(s => TableOperation.Delete(
            new DynamicTableEntity(s.PartitionKey, s.RowKey)
            {
                ETag = "*"
            })), token);
    }

    public async Task<bool> DeleteAllAsync(CancellationToken token = default)
    {
        //return _cloudTable.DropTable(token);
        return await Query.DeleteAsync(token) > 0;
    }

    #endregion

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }
}