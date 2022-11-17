using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using ManagedCode.Database.AzureTable.Extensions;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Exceptions;

namespace ManagedCode.Database.AzureTable;

public class AzureTableDatabaseCollection<TItem> : IDatabaseCollection<TableId, TItem>
    where TItem : AzureTableItem, new()
{
    private readonly TableClient _tableClient;

    public AzureTableDatabaseCollection(TableClient tableClient)
    {
        _tableClient = tableClient;
    }

    public ICollectionQueryable<TItem> Query => new AzureTableCollectionQueryable<TItem>(_tableClient);

    #region Get

    public async Task<TItem?> GetAsync(TableId id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await ExceptionCatcher.ExecuteAsync(_tableClient.GetEntityAsync<TItem>(id.PartitionKey,
                id.RowKey, cancellationToken: cancellationToken));

            return response.HasValue ? response.Value : null;
        }
        catch (DatabaseException e) when (e.InnerException is RequestFailedException { Status: 404 })
        {
            return null;
        }
    }

    #endregion

    #region Count

    public async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var query = _tableClient.QueryAsync<TItem>(cancellationToken: cancellationToken);
        return await query.LongCountAsync(cancellationToken);
    }

    #endregion

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    #region Insert

    public async Task<TItem?> InsertAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var response = await ExceptionCatcher.ExecuteAsync(_tableClient.AddEntityAsync(item, cancellationToken));

        return response.IsError ? null : item;
    }

    public async Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        var responses = await _tableClient.SubmitTransactionByChunksAsync(items,
            TableTransactionActionType.Add, cancellationToken);

        return responses.Count(v => !v.IsError);
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
        var responses = await _tableClient.SubmitTransactionByChunksAsync(items,
            TableTransactionActionType.UpsertMerge, cancellationToken);

        return responses.Count(v => !v.IsError);
    }

    #endregion

    #region Update

    public async Task<TItem?> UpdateAsync(TItem item, CancellationToken cancellationToken = default)
    {
        if (item.ETag != ETag.All) item.ETag = ETag.All;

        var response = await ExceptionCatcher.ExecuteAsync(_tableClient.UpdateEntityAsync(item, item.ETag,
            cancellationToken: cancellationToken));

        return response.IsError ? null : item;
    }

    public async Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        var responses = await _tableClient.SubmitTransactionByChunksAsync(items,
            TableTransactionActionType.UpdateMerge, cancellationToken);

        return responses.Count(v => !v.IsError);
    }

    #endregion

    #region Delete

    public async Task<bool> DeleteAsync(TableId id, CancellationToken cancellationToken = default)
    {
        var response = await _tableClient
            .DeleteEntityAsync(id.PartitionKey, id.RowKey, ETag.All, cancellationToken);

        return response?.IsError is not true;
    }

    public async Task<bool> DeleteAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var response = await _tableClient
            .DeleteEntityAsync(item.PartitionKey, item.RowKey, ETag.All, cancellationToken);

        return response?.IsError is not true;
    }

    public Task<int> DeleteAsync(IEnumerable<TableId> ids, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public async Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        var responses = await _tableClient.SubmitTransactionByChunksAsync(items,
            TableTransactionActionType.Delete, cancellationToken);

        return responses.Count(v => !v.IsError);
    }

    public async Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default)
    {
        var response = await _tableClient.DeleteAsync(cancellationToken);

        return !response.IsError;
    }

    #endregion
}