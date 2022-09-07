using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace ManagedCode.Database.CosmosDB;

public class CosmosDBCollection<TItem> : BaseDBCollection<string, TItem>
    where TItem : CosmosDbItem, IItem<string>, new()
{
    private readonly int _capacity = 50;
    private readonly CosmosDbAdapter<TItem> _cosmosDbAdapter;
    private readonly bool _splitByType;
    private readonly bool _useItemIdAsPartitionKey;

    public CosmosDBCollection([NotNull] CosmosDbRepositoryOptions options)
    {
        _splitByType = options.SplitByType;
        _useItemIdAsPartitionKey = options.UseItemIdAsPartitionKey;
        _cosmosDbAdapter = new CosmosDbAdapter<TItem>(options.ConnectionString, options.CosmosClientOptions, options.DatabaseName, options.CollectionName);
    }

    private Expression<Func<TItem, bool>> SplitByType()
    {
        if (_splitByType)
        {
            return w => w.Type == typeof(TItem).Name;
        }

        return w => true;
    }

    #region Get

    protected override async Task<TItem> GetAsyncInternal(string id, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var feedIterator = container.GetItemLinqQueryable<TItem>()
            .Where(w => w.Id == id)
            .ToFeedIterator();
        using (var iterator = feedIterator)
        {
            if (iterator.HasMoreResults)
            {
                token.ThrowIfCancellationRequested();

                foreach (var item in await iterator.ReadNextAsync(token))
                {
                    return item;
                }
            }
        }

        return null;
    }

    #endregion

    #region Count

    protected override async Task<long> CountAsyncInternal(CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        return await container.GetItemLinqQueryable<TItem>().Where(SplitByType()).CountAsync(token);
    }

    #endregion

    public override IDBCollectionQueryable<TItem> Query()
    {
        var container = _cosmosDbAdapter.GetContainer().Result;
        var queryable = container.GetItemLinqQueryable<TItem>().Where(SplitByType());
        return new CosmosDBCollectionQueryable<TItem>(queryable);
    }

    #region Insert

    public override void Dispose()
    {
    }

    public override ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    protected override async Task<TItem> InsertAsyncInternal(TItem item, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();

        var result = await container.CreateItemAsync(item, item.PartitionKey, cancellationToken: token);
        return result.Resource;
    }

    protected override async Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = 0;

        var container = await _cosmosDbAdapter.GetContainer();

        var batch = new List<Task>(_capacity);
        foreach (var item in items)
        {
            token.ThrowIfCancellationRequested();
            batch.Add(container.CreateItemAsync(item, item.PartitionKey, cancellationToken: token)
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

    protected override async Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var result = await container.UpsertItemAsync(item, item.PartitionKey, cancellationToken: token);
        return result.Resource;
    }

    protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var count = 0;
        var batch = new List<Task>(_capacity);
        foreach (var item in items)
        {
            token.ThrowIfCancellationRequested();
            batch.Add(container.UpsertItemAsync(item, item.PartitionKey, cancellationToken: token)
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

    protected override async Task<TItem> UpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var result = await container.ReplaceItemAsync(item, item.Id, cancellationToken: token);
        return result.Resource;
    }

    protected override async Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var count = 0;
        var batch = new List<Task>(_capacity);
        foreach (var item in items)
        {
            token.ThrowIfCancellationRequested();
            batch.Add(container.ReplaceItemAsync(item, item.Id, cancellationToken: token)
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

    protected override async Task<bool> DeleteAsyncInternal(string id, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();

        if (_useItemIdAsPartitionKey)
        {
            var deleteItemResult = await container.DeleteItemAsync<TItem>(id, new PartitionKey(id), cancellationToken: token);
            return deleteItemResult != null;
        }

        var item = await GetAsync(id, token);
        if (item == null)
        {
            return false;
        }

        var result = await container.DeleteItemAsync<TItem>(item.Id, item.PartitionKey, cancellationToken: token);
        return result != null;
    }

    protected override async Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var result = await container.DeleteItemAsync<TItem>(item.Id, item.PartitionKey, cancellationToken: token);
        return result != null;
    }

    protected override async Task<int> DeleteAsyncInternal(IEnumerable<string> ids, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var count = 0;
        var batch = new List<Task>(_capacity);
        foreach (var item in ids)
        {
            token.ThrowIfCancellationRequested();
            batch.Add(DeleteAsync(item, token)
                .ContinueWith(task =>
                {
                    if (task.Result)
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

    protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var count = 0;
        var batch = new List<Task>(_capacity);
        foreach (var item in items)
        {
            token.ThrowIfCancellationRequested();
            batch.Add(container.DeleteItemAsync<TItem>(item.Id, item.PartitionKey, cancellationToken: token)
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

    protected override async Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
    {
        if (_splitByType)
        {
            var delete = await Query().Where(item => true).DeleteAsync(token);
            return delete > 0;
        }

        var container = await _cosmosDbAdapter.GetContainer();
        var result = await container.DeleteContainerAsync(cancellationToken: token);
        return result != null;
    }

    #endregion
}