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

    public async Task<TItem?> GetAsync(TableId id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response =
                await _tableClient.GetEntityAsync<TItem>(id.PartitionKey, id.RowKey,
                    cancellationToken: cancellationToken);

            return response.HasValue ? response.Value : null;
        }
        catch (RequestFailedException e) when (e.Status == 404)
        {
            return null;
        }
    }

    #endregion

    #region Count

    public async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var query = _tableClient.QueryAsync<TItem>(cancellationToken: cancellationToken);
        return await query.LongCountAsync(cancellationToken: cancellationToken);
    }

    #endregion

    #region Insert

    public async Task<TItem?> InsertAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var response = await ExceptionCatcher.ExecuteAsync(_tableClient.AddEntityAsync(item, cancellationToken));

        return response.IsError ? null : item;
    }

    public async Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        var actions = items.Select(item => _tableClient.AddEntityAsync(item, cancellationToken: cancellationToken));

        return await ExceptionCatcher.ExecuteBatchAsync(actions, cancellationToken: cancellationToken);
    }

    #endregion

    #region InsertOrUpdate

    public async Task<TItem?> InsertOrUpdateAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var response =
            await ExceptionCatcher.ExecuteAsync(
                _tableClient.UpsertEntityAsync(item, cancellationToken: cancellationToken));

        return response.IsError ? null : item;
    }

    public async Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var actions = items.Select(item => _tableClient.UpsertEntityAsync(item, cancellationToken: cancellationToken));

        return await ExceptionCatcher.ExecuteBatchAsync(actions, cancellationToken: cancellationToken);
    }

    #endregion

    #region Update

    public async Task<TItem?> UpdateAsync(TItem item, CancellationToken cancellationToken = default)
    {
        if (item.ETag != ETag.All)
        {
            item.ETag = ETag.All;
        }

        var response = await ExceptionCatcher.ExecuteAsync(_tableClient.UpdateEntityAsync(item, item.ETag,
            cancellationToken: cancellationToken));

        return response.IsError ? null : item;
    }

    public async Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        var actions =
            items.Select(i => _tableClient.UpdateEntityAsync(i, i.ETag, cancellationToken: cancellationToken));

        _tableClient()
        return await ExceptionCatcher.ExecuteBatchAsync(actions, cancellationToken);
    }

    #endregion

    #region Delete

    public async Task<bool> DeleteAsync(TableId id, CancellationToken cancellationToken = default)
    {
        var response = await _tableClient
                .DeleteEntityAsync(id.PartitionKey, id.RowKey, ETag.All, cancellationToken: cancellationToken);
            )
        return response?.IsError is not true;
    }

    public async Task<bool> DeleteAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var response = await _tableClient
            .DeleteEntityAsync(item.PartitionKey, item.RowKey, ETag.All, cancellationToken: cancellationToken);

        return response?.IsError is not true;
    }

    public async Task<int> DeleteAsync(IEnumerable<TableId> ids, CancellationToken cancellationToken = default)
    {
        var actions = ids
            .Select(id => _tableClient.DeleteEntityAsync(id.PartitionKey, id.RowKey, ETag.All, cancellationToken));

        return await ExceptionCatcher.ExecuteBatchAsync(actions, cancellationToken: cancellationToken);
    }

    public async Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        var actions = items
            .Select(item =>
                _tableClient.DeleteEntityAsync(item.PartitionKey, item.RowKey, ETag.All, cancellationToken));

        TableTransactionAction action = new TableTransactionAction(TableTransactionActionType.Delete);

        return await ExceptionCatcher.ExecuteBatchAsync(actions, cancellationToken: cancellationToken);
    }

    public async Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default)
    {
        var response = await _tableClient.DeleteAsync(cancellationToken);

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