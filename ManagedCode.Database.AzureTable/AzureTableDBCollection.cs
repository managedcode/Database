using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using ManagedCode.Database.Core;

namespace ManagedCode.Database.AzureTable;

public class AzureTableDBCollection<TItem> : IDBCollection<TableId, TItem>
    where TItem : AzureTableItem, new()
{
    private readonly TableClient _tableClient;

    public AzureTableDBCollection(TableClient tableClient)
    {
        _tableClient = tableClient;
    }

    public IDBCollectionQueryable<TItem> Query => new AzureTableDBCollectionQueryable<TItem>(_tableClient);

    #region Get

    public async Task<TItem?> GetAsync(TableId id, CancellationToken token = default)
    {
        try
        {
            var response =
                await _tableClient.GetEntityAsync<TItem>(id.PartitionKey, id.RowKey, cancellationToken: token);

            return response.HasValue ? response.Value : null;
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            return null;
        }
    }

    #endregion

    #region Count

    public async Task<long> CountAsync(CancellationToken token = default)
    {
        var query = _tableClient.QueryAsync<TItem>(cancellationToken: token);
        return await query.LongCountAsync(cancellationToken: token);
    }

    #endregion

    #region Insert

    public async Task<TItem?> InsertAsync(TItem item, CancellationToken token = default)
    {
        var response = await _tableClient.AddEntityAsync(item, token);

        return response.IsError ? null : item;
    }

    public async Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var actions = items.Select(item => _tableClient.AddEntityAsync(item, cancellationToken: token));

        return await BatchHelper.ExecuteAsync(actions, token: token);
    }

    #endregion

    #region InsertOrUpdate

    public async Task<TItem?> InsertOrUpdateAsync(TItem item, CancellationToken token = default)
    {
        var response = await _tableClient.UpsertEntityAsync(item, cancellationToken: token);

        return response.IsError ? null : item;
    }

    public async Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items,
        CancellationToken token = default)
    {
        var actions = items.Select(item => _tableClient.UpsertEntityAsync(item, cancellationToken: token));

        return await BatchHelper.ExecuteAsync(actions, token: token);
    }

    #endregion

    #region Update

    public async Task<TItem?> UpdateAsync(TItem item, CancellationToken token = default)
    {
        if (item.ETag != ETag.All)
        {
            item.ETag = ETag.All;
        }

        var response = await _tableClient.UpdateEntityAsync(item, item.ETag, cancellationToken: token);

        return response.IsError ? null : item;
    }

    public async Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var actions = items.Select(i => _tableClient.UpdateEntityAsync(i, i.ETag, cancellationToken: token));
        return await BatchHelper.ExecuteAsync(actions, token);
    }

    #endregion

    #region Delete

    public async Task<bool> DeleteAsync(TableId id, CancellationToken token = default)
    {
        var response = await _tableClient
            .DeleteEntityAsync(id.PartitionKey, id.RowKey, ETag.All, cancellationToken: token);

        return response?.IsError is not true;
    }

    public async Task<bool> DeleteAsync(TItem item, CancellationToken token = default)
    {
        var response = await _tableClient
            .DeleteEntityAsync(item.PartitionKey, item.RowKey, ETag.All, cancellationToken: token);

        return response?.IsError is not true;
    }

    public async Task<int> DeleteAsync(IEnumerable<TableId> ids, CancellationToken token = default)
    {
        var actions = ids
            .Select(id => _tableClient.DeleteEntityAsync(id.PartitionKey, id.RowKey, ETag.All, token));

        return await BatchHelper.ExecuteAsync(actions, token: token);
    }

    public async Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var actions = items
            .Select(item => _tableClient.DeleteEntityAsync(item.PartitionKey, item.RowKey, ETag.All, token));

        return await BatchHelper.ExecuteAsync(actions, token: token);
    }

    public async Task<bool> DeleteCollectionAsync(CancellationToken token = default)
    {
        var response = await _tableClient.DeleteAsync(token);

        return !response.IsError;
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