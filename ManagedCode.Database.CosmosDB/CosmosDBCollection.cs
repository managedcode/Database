/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace ManagedCode.Database.CosmosDB;

public class CosmosDBCollection<TItem> : IDBCollection<string, TItem>
    where TItem : CosmosDbItem, IItem<string>, new()
{
    private readonly int _capacity = 50;
    private readonly CosmosDbAdapter<TItem> _cosmosDbAdapter;
    private readonly bool _splitByType;
    private readonly bool _useItemIdAsPartitionKey;

    public CosmosDBCollection(CosmosDbRepositoryOptions options)
    {
        _splitByType = options.SplitByType;
        _useItemIdAsPartitionKey = options.UseItemIdAsPartitionKey;
        _cosmosDbAdapter = new CosmosDbAdapter<TItem>(options.ConnectionString, options.CosmosClientOptions,
            options.DatabaseName, options.CollectionName);
    }

    public IDBCollectionQueryable<TItem> Query
    {
        get
        {
            var container = _cosmosDbAdapter.GetContainer().Result;
            if (!_splitByType)
            {
                return new CosmosDBCollectionQueryable<TItem>(container.GetItemLinqQueryable<TItem>());
            }

            var queryable = container.GetItemLinqQueryable<TItem>().Where(SplitByType());
            return new CosmosDBCollectionQueryable<TItem>(queryable);
        }
    }

    private Expression<Func<TItem, bool>> SplitByType()
    {
        return w => w.Type == typeof(TItem).Name;
    }

    #region Get

    public async Task<TItem?> GetAsync(string id, CancellationToken token = default)
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

    public async Task<long> CountAsync(CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        return await container.GetItemLinqQueryable<TItem>().Where(SplitByType()).CountAsync(token);
    }

    #endregion

    #region Insert

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    public async Task<TItem> InsertAsync(TItem item, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();

        var result = await container.CreateItemAsync(item, item.PartitionKey, cancellationToken: token);
        return result.Resource;
    }

    public async Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken token = default)
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

    public async Task<TItem> InsertOrUpdateAsync(TItem item, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var result = await container.UpsertItemAsync(item, item.PartitionKey, cancellationToken: token);
        return result.Resource;
    }

    public async Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items, CancellationToken token = default)
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

    public async Task<TItem> UpdateAsync(TItem item, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var result = await container.ReplaceItemAsync(item, item.Id, cancellationToken: token);
        return result.Resource;
    }

    public async Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken token = default)
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

    public async Task<bool> DeleteAsync(string id, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();

        if (_useItemIdAsPartitionKey)
        {
            var deleteItemResult =
                await container.DeleteItemAsync<TItem>(id, new PartitionKey(id), cancellationToken: token);
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

    public async Task<bool> DeleteAsync(TItem item, CancellationToken token = default)
    {
        var container = await _cosmosDbAdapter.GetContainer();
        var result = await container.DeleteItemAsync<TItem>(item.Id, item.PartitionKey, cancellationToken: token);
        return result != null;
    }

    public async Task<int> DeleteAsync(IEnumerable<string> ids, CancellationToken token = default)
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

    public async Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken token = default)
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

    public async Task<bool> DeleteAllAsync(CancellationToken token = default)
    {
        if (_splitByType)
        {
            var delete = await Query.Where(item => true).DeleteAsync(token);
            return delete > 0;
        }

        var container = await _cosmosDbAdapter.GetContainer();
        var result = await container.DeleteContainerAsync(cancellationToken: token);
        return result != null;
    }

    #endregion
}*/