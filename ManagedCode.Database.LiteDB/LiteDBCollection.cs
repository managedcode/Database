using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Exceptions;

namespace ManagedCode.Database.LiteDB;

public class LiteDBCollection<TId, TItem> : BaseDatabaseCollection<TId, TItem>
    where TItem : LiteDBItem<TId>, IItem<TId>, new()
{
    private readonly ILiteCollection<TItem> _collection;

    public LiteDBCollection(ILiteCollection<TItem> collection)
    {
        _collection = collection;
    }

    public override ICollectionQueryable<TItem> Query => new LiteDBCollectionQueryable<TId, TItem>(_collection);

    public override ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    public override void Dispose()
    {
    }

    #region Get

    protected override async Task<TItem?> GetInternalAsync(TId id, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _collection.FindById(new BsonValue(id));
    }

    #endregion

    #region Count

    protected override async Task<long> CountInternalAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => _collection.Count(), cancellationToken);
    }

    #endregion

    #region Insert

    protected override async Task<TItem> InsertInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        var bson = _collection.Insert(item);
        return _collection.FindById(bson);
    }

    protected override async Task<int> InsertInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => _collection.InsertBulk(items), cancellationToken);
    }

    #endregion

    #region InsertOrUpdate

    protected override async Task<TItem> InsertOrUpdateInternalAsync(TItem item,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            _collection.Upsert(item);
            return _collection.FindById(new BsonValue(item.Id));
        }, cancellationToken);
    }

    protected override async Task<int> InsertOrUpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => _collection.Upsert(items), cancellationToken);
    }

    #endregion

    #region Update

    protected override async Task<TItem> UpdateInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        var isUpdated = _collection.Update(item);

        if (!isUpdated)
        {
            throw new DatabaseException("Entity not found in collection.");
        }

        return _collection.FindById(new BsonValue(item.Id));
    }

    protected override async Task<int> UpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _collection.Update(items);
    }

    #endregion

    #region Delete

    protected override async Task<bool> DeleteInternalAsync(TId id, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _collection.Delete(new BsonValue(id));
    }

    protected override async Task<bool> DeleteInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _collection.Delete(new BsonValue(item.Id));
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<TId> ids,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => ids.Count(id => _collection.Delete(new BsonValue(id))), cancellationToken);
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => items.Count(item => _collection.Delete(new BsonValue(item.Id))),
            cancellationToken);
    }

    protected override async Task<bool> DeleteCollectionInternalAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(() => _collection.DeleteAll(), cancellationToken);
        return await CountInternalAsync(cancellationToken) == 0;
    }

    #endregion
}