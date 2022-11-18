using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ManagedCode.Database.MongoDB;

public class MongoDBDatabaseCollection<TItem> : BaseDatabaseCollection<ObjectId, TItem>
    where TItem : class, IItem<ObjectId>
{
    private readonly IMongoCollection<TItem> _collection;

    public MongoDBDatabaseCollection(IMongoCollection<TItem> collection)
    {
        _collection = collection;
    }

    public override ICollectionQueryable<TItem> Query => new MongoDBCollectionQueryable<TItem>(_collection);

    public override void Dispose()
    {
    }

    public override ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    #region Get

    protected override async Task<TItem?> GetInternalAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        var cursor = await _collection.FindAsync(w => w.Id == id, cancellationToken: cancellationToken);

        return await cursor.FirstOrDefaultAsync(cancellationToken);
    }

    #endregion

    #region Count

    protected override async Task<long> CountInternalAsync(CancellationToken cancellationToken = default)
    {
        var task = _collection.CountDocumentsAsync(f => true, new CountOptions(), cancellationToken);
        return await task;
    }

    #endregion

    #region Insert

    protected override async Task<TItem> InsertInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(item, new InsertOneOptions(), cancellationToken);

        return item;
    }

    protected override async Task<int> InsertInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        await _collection.InsertManyAsync(items, new InsertManyOptions(),
            cancellationToken);

        return items.Count();
    }

    #endregion

    #region InsertOrUpdate

    protected override async Task<TItem> InsertOrUpdateInternalAsync(TItem item,
        CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(w => w.Id == item.Id, item, new ReplaceOptions
        {
            IsUpsert = true
        }, cancellationToken);

        return item;
    }

    protected override async Task<int> InsertOrUpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var count = 0;

        foreach (var item in items)
        {
            await InsertOrUpdateInternalAsync(item, cancellationToken);
            count++;
        }

        return count;
    }

    #endregion

    #region Update

    protected override async Task<TItem> UpdateInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var result = await _collection.ReplaceOneAsync(Builders<TItem>.Filter.Eq("_id", item.Id), item,
            cancellationToken: cancellationToken);

        if (result.ModifiedCount == 0)
        {
            throw new DatabaseException("Entity not found in collection.");
        }

        return item;
    }

    protected override async Task<int> UpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var count = 0;

        foreach (var item in items)
        {
            await UpdateInternalAsync(item, cancellationToken);
            count++;
        }

        return count;
    }

    #endregion

    #region Delete

    protected override async Task<bool> DeleteInternalAsync(ObjectId id, CancellationToken cancellationToken = default)
    {
        var item = await _collection.FindOneAndDeleteAsync<TItem>(w => w.Id == id, new FindOneAndDeleteOptions<TItem>(),
            cancellationToken);

        return item is not null;
    }

    protected override async Task<bool> DeleteInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var result = await _collection.FindOneAndDeleteAsync<TItem>(w => w.Id == item.Id,
            new FindOneAndDeleteOptions<TItem>(), cancellationToken);

        return result is not null;
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<ObjectId> ids,
        CancellationToken cancellationToken = default)
    {
        var count = 0;
        foreach (var item in ids)
            if (await DeleteInternalAsync(item, cancellationToken))
                count++;

        return count;
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var count = 0;
        foreach (var item in items)
            if (await DeleteInternalAsync(item, cancellationToken))
                count++;

        return count;
    }

    protected override async Task<bool> DeleteCollectionInternalAsync(CancellationToken cancellationToken = default)
    {
        var result = await _collection.DeleteManyAsync(w => true, cancellationToken);
        return result.DeletedCount > 0;
    }

    #endregion
}