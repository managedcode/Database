using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using ManagedCode.Database.AzureTables.Extensions;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Exceptions;

namespace ManagedCode.Database.AzureTables;

public class AzureTablesDatabaseCollection<TItem> : BaseDatabaseCollection<TableId, TItem>
    where TItem : AzureTablesItem, new()
{
    private readonly TableClient _tableClient;

    public AzureTablesDatabaseCollection(TableClient tableClient)
    {
        _tableClient = tableClient;
    }

    public override ICollectionQueryable<TItem> Query => new AzureTablesCollectionQueryable<TItem>(_tableClient);

    public override void Dispose()
    {
    }

    public override ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    #region Get

    protected override async Task<TItem?> GetInternalAsync(TableId id, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<TItem>(id.PartitionKey,
                id.RowKey, cancellationToken: cancellationToken);

            return response.HasValue ? response.Value : null;
        }
        catch (RequestFailedException e) when (e.Status == 404) 
        {
            return null;
        }
    }

    #endregion

    #region Count

    protected override async Task<long> CountInternalAsync(CancellationToken cancellationToken = default)
    {
        var query = _tableClient.QueryAsync<TItem>(cancellationToken: cancellationToken);
        return await query.LongCountAsync(cancellationToken);
    }

    #endregion

    #region Insert

    protected override async Task<TItem> InsertInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var response = await _tableClient.AddEntityAsync(item, cancellationToken);

        return response.IsError ? null : item;
    }

    protected override async Task<int> InsertInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var responses = await _tableClient.SubmitTransactionByChunksAsync(items,
            TableTransactionActionType.Add, cancellationToken);

        return responses.Count(v => !v.IsError);
    }

    #endregion

    #region InsertOrUpdate

    protected override async Task<TItem> InsertOrUpdateInternalAsync(TItem item,
        CancellationToken cancellationToken = default)
    {
        var response =
            await
                _tableClient.UpsertEntityAsync(item, cancellationToken: cancellationToken);

        return response.IsError ? null : item;
    }

    protected override async Task<int> InsertOrUpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var responses = await _tableClient.SubmitTransactionByChunksAsync(items,
            TableTransactionActionType.UpsertReplace, cancellationToken);

        return responses.Count(v => !v.IsError);
    }

    #endregion

    #region Update

    protected override async Task<TItem> UpdateInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        if (item.ETag != ETag.All) item.ETag = ETag.All;

        var response = await _tableClient.UpdateEntityAsync(item, item.ETag,
            cancellationToken: cancellationToken);

        return response.IsError ? null : item;
    }

    protected override async Task<int> UpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var responses = await _tableClient.SubmitTransactionByChunksAsync(items,
            TableTransactionActionType.UpsertReplace, cancellationToken);

        return responses.Count(v => !v.IsError);
    }

    #endregion

    #region Delete

    protected override async Task<bool> DeleteInternalAsync(TableId id, CancellationToken cancellationToken = default)
    {
        var response = await _tableClient
            .DeleteEntityAsync(id.PartitionKey, id.RowKey, ETag.All, cancellationToken);

        return response?.IsError is not true;
    }

    protected override async Task<bool> DeleteInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var response = await _tableClient
            .DeleteEntityAsync(item.PartitionKey, item.RowKey, ETag.All, cancellationToken);

        return response?.IsError is not true;
    }

    protected override Task<int> DeleteInternalAsync(IEnumerable<TableId> ids,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var responses = await _tableClient.SubmitTransactionByChunksAsync(items,
            TableTransactionActionType.Delete, cancellationToken);

        return responses.Count(v => !v.IsError);
    }

    protected override async Task<bool> DeleteCollectionInternalAsync(CancellationToken cancellationToken = default)
    {
        var response = await _tableClient.DeleteAsync(cancellationToken);

        return !response.IsError;
    }

    #endregion
}