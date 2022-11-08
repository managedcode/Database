using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ManagedCode.Database.MongoDB;

public class MongoDbCollection<TItem> : IDBCollection<ObjectId, TItem>
    where TItem : class, IItem<ObjectId>
{
    private readonly IMongoCollection<TItem> _collection;

    public MongoDbCollection(IMongoCollection<TItem> collection)
    {
        _collection = collection;
    }

    public IDBCollectionQueryable<TItem> Query => new MongoDbDBCollectionQueryable<TItem>(_collection);

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    #region Get

    public async Task<TItem> GetAsync(ObjectId id, CancellationToken token = default)
    {
        var cursor = await _collection.FindAsync(w => w.Id == id, cancellationToken: token);
        return cursor.FirstOrDefault();
    }

    #endregion

    #region Insert

    public async Task<TItem> InsertAsync(TItem item, CancellationToken token = default)
    {
        await _collection.InsertOneAsync(item, new InsertOneOptions(), token);
        return item;
    }

    public async Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        await _collection.InsertManyAsync(items, new InsertManyOptions(), token);
        return items.Count();
    }

    #endregion

    #region InsertOrUpdate

    public async Task<TItem> InsertOrUpdateAsync(TItem item, CancellationToken token = default)
    {
        var result = await _collection.ReplaceOneAsync(w => w.Id == item.Id, item, new ReplaceOptions
        {
            IsUpsert = true
        }, token);

        return item;
    }

    public async Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = 0;
        foreach (var item in items)
        {
            await InsertOrUpdateAsync(item, token);
            count++;
        }

        return count;
    }

    #endregion

    #region Update

    public async Task<TItem> UpdateAsync(TItem item, CancellationToken token = default)
    {
        var r = await _collection.ReplaceOneAsync(Builders<TItem>.Filter.Eq("_id", item.Id), item,
            cancellationToken: token);
        return item;
    }

    public async Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = 0;
        foreach (var item in items)
        {
            await UpdateAsync(item, token);
            count++;
        }

        return count;
    }

    #endregion

    #region Delete

    public async Task<bool> DeleteAsync(ObjectId id, CancellationToken token = default)
    {
        var item = await _collection.FindOneAndDeleteAsync<TItem>(w => w.Id == id, new FindOneAndDeleteOptions<TItem>(),
            token);
        return item != null;
    }

    public async Task<bool> DeleteAsync(TItem item, CancellationToken token = default)
    {
        var i = await _collection.FindOneAndDeleteAsync<TItem>(w => w.Id == item.Id,
            new FindOneAndDeleteOptions<TItem>(), token);
        return i != null;
    }

    public async Task<int> DeleteAsync(IEnumerable<ObjectId> ids, CancellationToken token = default)
    {
        var count = 0;
        foreach (var item in ids)
        {
            if (await DeleteAsync(item, token))
            {
                count++;
            }
        }

        return count;
    }

    public async Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var count = 0;
        foreach (var item in items)
        {
            if (await DeleteAsync(item, token))
            {
                count++;
            }
        }

        return count;
    }

    public async Task<bool> DeleteAllAsync(CancellationToken token = default)
    {
        var result = await _collection.DeleteManyAsync(w => true, token);
        return result.DeletedCount > 0;
    }

    #endregion

    #region Count

    public async Task<long> CountAsync(CancellationToken token = default)
    {
        return Convert.ToInt32(await _collection.CountDocumentsAsync(f => true, new CountOptions(), token));
    }

    #endregion
}