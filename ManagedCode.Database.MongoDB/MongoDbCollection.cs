using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ManagedCode.Database.MongoDB;

public class MongoDbCollection<TItem> : BaseDBCollection<ObjectId, TItem>
    where TItem : class, IItem<ObjectId>
{
    private readonly IMongoCollection<TItem> _collection;

    public MongoDbCollection(IMongoCollection<TItem> collection)
    {
        _collection = collection;
    }

    public override void Dispose()
    {
    }

    public override ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    #region Get

    protected override async Task<TItem> GetAsyncInternal(ObjectId id, CancellationToken token = default)
    {
        var cursor = await _collection.FindAsync(w => w.Id == id, cancellationToken: token);
        return cursor.FirstOrDefault();
    }

    #endregion

    #region Insert

    protected override async Task<TItem> InsertAsyncInternal(TItem item, CancellationToken token = default)
    {
        await _collection.InsertOneAsync(item, new InsertOneOptions(), token);
        return item;
    }

    protected override async Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        await _collection.InsertManyAsync(items, new InsertManyOptions(), token);
        return items.Count();
    }

    #endregion

    #region InsertOrUpdate

    protected override async Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        var result = await _collection.ReplaceOneAsync(w => w.Id == item.Id, item, new ReplaceOptions
        {
            IsUpsert = true
        }, token);

        return item;
    }

    protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = 0;
        foreach (var item in items)
        {
            await InsertOrUpdateAsyncInternal(item, token);
            count++;
        }

        return count;
    }

    #endregion

    #region Update

    protected override async Task<TItem> UpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        var r = await _collection.ReplaceOneAsync(Builders<TItem>.Filter.Eq("_id", item.Id), item, cancellationToken: token);
        return item;
    }

    protected override async Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = 0;
        foreach (var item in items)
        {
            await UpdateAsyncInternal(item, token);
            count++;
        }

        return count;
    }

    #endregion

    #region Delete

    protected override async Task<bool> DeleteAsyncInternal(ObjectId id, CancellationToken token = default)
    {
        var item = await _collection.FindOneAndDeleteAsync<TItem>(w => w.Id == id, new FindOneAndDeleteOptions<TItem>(), token);
        return item != null;
    }

    protected override async Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
    {
        var i = await _collection.FindOneAndDeleteAsync<TItem>(w => w.Id == item.Id, new FindOneAndDeleteOptions<TItem>(), token);
        return i != null;
    }

    protected override async Task<int> DeleteAsyncInternal(IEnumerable<ObjectId> ids, CancellationToken token = default)
    {
        var count = 0;
        foreach (var item in ids)
        {
            if (await DeleteAsyncInternal(item, token))
            {
                count++;
            }
        }

        return count;
    }

    protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = 0;
        foreach (var item in items)
        {
            if (await DeleteAsyncInternal(item, token))
            {
                count++;
            }
        }

        return count;
    }

    protected override async Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
    {
        var result = await _collection.DeleteManyAsync(w => true, token);
        return result.DeletedCount > 0;
    }

    #endregion

    #region Count

    public override IDBCollectionQueryable<TItem> Query()
    {
        return new MongoDbDBCollectionQueryable<TItem>(_collection);
    }

    protected override async Task<long> CountAsyncInternal(CancellationToken token = default)
    {
        return Convert.ToInt32(await _collection.CountDocumentsAsync(f => true, new CountOptions(), token));
    }

    #endregion
}