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
    private const int Capacity = 50;
    private readonly TableClient _tableClient;

    public AzureTableDBCollection(TableClient tableClient)
    {
        _tableClient = tableClient;
    }

    public IDBCollectionQueryable<TItem> Query => new AzureTableDBCollectionQueryable<TItem>(_tableClient);

    #region Get

    public async Task<TItem?> GetAsync(TableId id, CancellationToken token = default)
    {
        var response = await _tableClient.GetEntityAsync<TItem>(id.PartitionKey, id.RowKey, cancellationToken: token);

        return response.HasValue ? response.Value : null;
    }

    #endregion

    #region Count

    public async Task<long> CountAsync(CancellationToken token = default)
    {
        //
        // var query = new TableQuery<DynamicTableEntity>();
        // query = query.CustomSelect(item => new DynamicTableEntity(item.PartitionKey, item.RowKey));
        //
        // return await _tableClient.ExecuteQuery(query, cancellationToken: token).CountAsync(token);

        return 0;
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
        var count = 0;

        var batch = new List<Task>(Capacity);
        foreach (var item in items)
        {
            token.ThrowIfCancellationRequested();
            batch.Add(_tableClient.AddEntityAsync(item, cancellationToken: token)
                .ContinueWith(task =>
                {
                    if (task.Result != null)
                    {
                        Interlocked.Increment(ref count);
                    }
                }, token));

            if (count == batch.Capacity)
            {
                await Task.WhenAll(batch);
                batch.Clear();
            }
        }

        token.ThrowIfCancellationRequested();

        if (batch.Count > 0)
        {
            await Task.WhenAll(batch);
            batch.Clear();
        }

        return count;
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
        var count = 0;
        var batch = new List<Task>(Capacity);
        foreach (var item in items)
        {
            token.ThrowIfCancellationRequested();
            batch.Add(_tableClient.UpsertEntityAsync(item, cancellationToken: token)
                .ContinueWith(task =>
                {
                    if (task.Result != null)
                    {
                        Interlocked.Increment(ref count);
                    }
                }, token));

            if (count == batch.Capacity)
            {
                await Task.WhenAll(batch);
                batch.Clear();
            }
        }

        token.ThrowIfCancellationRequested();

        if (batch.Count > 0)
        {
            await Task.WhenAll(batch);
            batch.Clear();
        }

        return count;
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
        var count = 0;
        var batch = new List<Task>(Capacity);
        foreach (var item in items)
        {
            token.ThrowIfCancellationRequested();

            if (item.ETag != ETag.All)
            {
                item.ETag = ETag.All;
            }

            batch.Add(_tableClient.UpdateEntityAsync(item, item.ETag, cancellationToken: token)
                .ContinueWith(task =>
                {
                    if (task.Result != null)
                    {
                        Interlocked.Increment(ref count);
                    }
                }, token));

            if (count == batch.Capacity)
            {
                await Task.WhenAll(batch);
                batch.Clear();
            }
        }

        token.ThrowIfCancellationRequested();

        if (batch.Count > 0)
        {
            await Task.WhenAll(batch);
            batch.Clear();
        }

        return count;
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
        var count = 0;
        var batch = new List<Task>(Capacity);
        foreach (var id in ids)
        {
            token.ThrowIfCancellationRequested();
            batch.Add(_tableClient.DeleteEntityAsync(id.PartitionKey, id.RowKey, ETag.All, cancellationToken: token)
                .ContinueWith(task =>
                {
                    if (task.Result != null)
                    {
                        Interlocked.Increment(ref count);
                    }
                }, token));

            if (count == batch.Capacity)
            {
                await Task.WhenAll(batch);
                batch.Clear();
            }
        }

        token.ThrowIfCancellationRequested();

        if (batch.Count > 0)
        {
            await Task.WhenAll(batch);
            batch.Clear();
        }

        return count;
    }

    public async Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = 0;
        var batch = new List<Task>(Capacity);
        foreach (var item in items)
        {
            token.ThrowIfCancellationRequested();
            batch.Add(_tableClient.DeleteEntityAsync(item.PartitionKey, item.RowKey, cancellationToken: token)
                .ContinueWith(task =>
                {
                    if (task.Result != null)
                    {
                        Interlocked.Increment(ref count);
                    }
                }, token));

            if (count == batch.Capacity)
            {
                await Task.WhenAll(batch);
                batch.Clear();
            }
        }

        token.ThrowIfCancellationRequested();

        if (batch.Count > 0)
        {
            await Task.WhenAll(batch);
            batch.Clear();
        }

        return count;
    }

    public async Task<bool> DeleteAllAsync(CancellationToken token = default)
    {
        // return await Query.DeleteAsync(token) > 0;

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