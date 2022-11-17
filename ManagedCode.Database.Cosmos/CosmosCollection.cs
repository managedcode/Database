using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace ManagedCode.Database.Cosmos;

public class CosmosCollection<TItem> : IDatabaseCollection<string, TItem>
    where TItem : CosmosItem, IItem<string>, new()
{
    private const int Capacity = 50;
    private readonly Container _container;

    private readonly bool _splitByType;
    private readonly bool _useItemIdAsPartitionKey;

    public CosmosCollection(CosmosOptions options, Container container)
    {
        _container = container;
        _splitByType = options.SplitByType;
        _useItemIdAsPartitionKey = options.UseItemIdAsPartitionKey;
    }

    public ICollectionQueryable<TItem> Query
    {
        get
        {
            if (!_splitByType)
                return new CosmosCollectionQueryable<TItem>(_container, _container.GetItemLinqQueryable<TItem>());

            var queryable = _container.GetItemLinqQueryable<TItem>().Where(SplitByType());
            return new CosmosCollectionQueryable<TItem>(_container, queryable);
        }
    }

    #region Get

    public async Task<TItem?> GetAsync(string id, CancellationToken cancellationToken = default)
    {
        var query = _container.GetItemLinqQueryable<TItem>();
        return await Task.Run(() => query.FirstOrDefault(w => w.Id == id), cancellationToken);
    }

    #endregion

    #region Count

    public async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _container.GetItemLinqQueryable<TItem>()
            .Where(SplitByType())
            .CountAsync(cancellationToken);
    }

    #endregion

    private Expression<Func<TItem, bool>> SplitByType()
    {
        return w => w.Type == typeof(TItem).Name;
    }

    #region Insert

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    public async Task<TItem> InsertAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var result = await _container.CreateItemAsync(item, item.PartitionKey, cancellationToken: cancellationToken);
        return result.Resource;
    }

    public async Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        var count = 0;

        await Parallel.ForEachAsync(items, cancellationToken, async (item, token) =>
        {
            var response =
                await _container.CreateItemAsync(item, item.PartitionKey, cancellationToken: token);

            if (response is not null) Interlocked.Increment(ref count);
        });

        return count;
    }

    #endregion

    #region InsertOrUpdate

    public async Task<TItem> InsertOrUpdateAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var result = await _container.UpsertItemAsync(item, item.PartitionKey, cancellationToken: cancellationToken);
        return result.Resource;
    }

    public async Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        var count = 0;

        await Parallel.ForEachAsync(items, cancellationToken, async (item, token) =>
        {
            var response =
                await _container.UpsertItemAsync(item, item.PartitionKey, cancellationToken: token);

            if (response is not null) Interlocked.Increment(ref count);
        });

        return count;
    }

    #endregion

    #region Update

    public async Task<TItem> UpdateAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var result = await _container.ReplaceItemAsync(item, item.Id, cancellationToken: cancellationToken);
        return result.Resource;
    }

    public async Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        var count = 0;

        await Parallel.ForEachAsync(items, cancellationToken, async (item, token) =>
        {
            var response =
                await _container.ReplaceItemAsync(item, item.Id, cancellationToken: token);

            if (response is not null) Interlocked.Increment(ref count);
        });

        return count;
    }

    #endregion

    #region Delete

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_useItemIdAsPartitionKey)
        {
            var deleteItemResult =
                await _container.DeleteItemAsync<TItem>(id, new PartitionKey(id), cancellationToken: cancellationToken);
            return deleteItemResult != null;
        }

        var item = await GetAsync(id, cancellationToken);

        if (item is null) return false;

        var result =
            await _container.DeleteItemAsync<TItem>(item.Id, item.PartitionKey, cancellationToken: cancellationToken);
        return result is not null;
    }

    public async Task<bool> DeleteAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var result =
            await _container.DeleteItemAsync<TItem>(item.Id, item.PartitionKey, cancellationToken: cancellationToken);

        return result is not null;
    }

    public async Task<int> DeleteAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        var count = 0;
        var batch = new List<Task>(Capacity);
        foreach (var item in ids)
        {
            cancellationToken.ThrowIfCancellationRequested();
            batch.Add(DeleteAsync(item, cancellationToken)
                .ContinueWith(task =>
                {
                    if (task.Result) Interlocked.Increment(ref count);
                }, cancellationToken));

            if (count == batch.Capacity)
            {
                await Task.WhenAll(batch);
                batch.Clear();
            }
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (batch.Count > 0)
        {
            await Task.WhenAll(batch);
            batch.Clear();
        }

        return count;
    }

    public async Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        var count = 0;

        await Parallel.ForEachAsync(items, cancellationToken, async (item, token) =>
        {
            var response =
                await _container.DeleteItemAsync<TItem>(item.Id, item.PartitionKey, cancellationToken: token);

            if (response is not null) Interlocked.Increment(ref count);
        });

        return count;
    }

    public async Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default)
    {
        if (_splitByType)
        {
            var delete = await Query.Where(item => true).DeleteAsync(cancellationToken);
            return delete > 0;
        }

        var result = await _container.DeleteContainerAsync(cancellationToken: cancellationToken);
        return result != null;
    }

    #endregion
}