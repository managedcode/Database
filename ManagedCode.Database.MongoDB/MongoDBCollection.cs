using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Exceptions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ManagedCode.Database.MongoDB;

public class MongoDBCollection<TItem> : BaseDatabaseCollection<ObjectId, TItem>
    where TItem : MongoDBItem
{
    private readonly IMongoCollection<TItem> _collection;

    public MongoDBCollection(IMongoCollection<TItem> collection)
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

    protected override async Task<List<TItem>> GetCollectionInternalAsync(CancellationToken cancellationToken = default)
    {
        var collectionName = _collection.CollectionNamespace.CollectionName;

        var collection = await _collection.Database.GetCollection<TItem>(collectionName).Find<TItem>(_ => true).ToListAsync();

        return collection;
    }

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
        var models = items.Select(i => new InsertOneModel<TItem>(i));

        var result = await _collection.BulkWriteAsync(models, new BulkWriteOptions(), cancellationToken);

        return (int) result.InsertedCount;
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

        return (await GetInternalAsync(item.Id, cancellationToken))!;
    }

    protected override async Task<int> InsertOrUpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var models = items.Select(item => new ReplaceOneModel<TItem>(Builders<TItem>.Filter.Eq(nameof(item.Id), item.Id), item) {IsUpsert = true})
            .ToList();

        var result = await _collection.BulkWriteAsync(models, new BulkWriteOptions(), cancellationToken);

        return (int) Math.Max(result.Upserts.Count, result.ModifiedCount);
    }

    #endregion

    #region Update

    protected override async Task<TItem> UpdateInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var result = await _collection.ReplaceOneAsync(Builders<TItem>.Filter.Eq(nameof(item.Id), item.Id), item,
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
        var models = items.Select(item => new ReplaceOneModel<TItem>(Builders<TItem>.Filter.Eq(nameof(item.Id), item.Id), item)).ToList();

        var result = await _collection.BulkWriteAsync(models, new BulkWriteOptions(), cancellationToken);

        return (int) result.MatchedCount;
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
        var models = ids
            .Select(id => new DeleteOneModel<TItem>(Builders<TItem>.Filter.Eq(nameof(MongoDBItem.Id), id)));

        var result = await _collection.BulkWriteAsync(models, new BulkWriteOptions(), cancellationToken);

        return (int) result.DeletedCount;
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var models = items
            .Select(item => new DeleteOneModel<TItem>(Builders<TItem>.Filter.Eq(nameof(item.Id), item.Id)));

        var result = await _collection.BulkWriteAsync(models, new BulkWriteOptions(), cancellationToken);

        return (int) result.DeletedCount;
    }

    protected override async Task<bool> DeleteCollectionInternalAsync(CancellationToken cancellationToken = default)
    {
        await _collection.DeleteManyAsync(w => true, cancellationToken);

        return await CountInternalAsync(cancellationToken) == 0;
    }

    #endregion
}