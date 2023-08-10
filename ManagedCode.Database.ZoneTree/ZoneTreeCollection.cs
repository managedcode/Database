using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Exceptions;

namespace ManagedCode.Database.ZoneTree;

public class ZoneTreeCollection<TId, TItem> : BaseDatabaseCollection<TId, TItem> where TItem : IItem<TId>
{
    private readonly ZoneTreeWrapper<TId, TItem> _zoneTree;

    internal ZoneTreeCollection(ZoneTreeCollectionOptions<TId, TItem> options)
    {
        _zoneTree = new ZoneTreeWrapper<TId, TItem>(options);
    }

    public override ICollectionQueryable<TItem> Query => new ZoneTreeCollectionQueryable<TId, TItem>(_zoneTree);


    public override void Dispose()
    {
        _zoneTree.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        _zoneTree.Dispose();
        return ValueTask.CompletedTask;
    }

    protected override async Task<TItem> InsertInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        if (!_zoneTree.Insert(item.Id, item))
        {
            throw new DatabaseException("The specified entity already exists.");
        }

        return _zoneTree.Get(item.Id)!;
    }

    protected override async Task<int> InsertInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => items.Count(item => _zoneTree.Insert(item.Id, item)), cancellationToken);
    }

    protected override async Task<TItem> UpdateInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        if (!_zoneTree.Update(item.Id, item))
        {
            throw new DatabaseException("Entity not found in collection.");
        }

        return _zoneTree.Get(item.Id)!;
    }

    protected override async Task<int> UpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => items.Count(item => _zoneTree.Update(item.Id, item)), cancellationToken);
    }

    protected override async Task<bool> DeleteInternalAsync(TId id, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _zoneTree.Delete(id);
    }

    protected override async Task<bool> DeleteInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _zoneTree.Delete(item.Id);
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<TId> ids,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => ids.Count(id => _zoneTree.Delete(id)), cancellationToken);
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => items.Count(item => _zoneTree.Delete(item.Id)), cancellationToken);
    }

    protected override Task<bool> DeleteCollectionInternalAsync(CancellationToken cancellationToken = default)
    {
        _zoneTree.DeleteAll();
        return Task.FromResult(true);
    }

    protected override Task<List<TItem>> GetCollectionInternalAsync(CancellationToken cancellationToken = default)
    {
        var collection = _zoneTree.Enumerate().ToList();
        return Task.FromResult(collection.Count == 0 ? null : collection)!;
    }

    protected override async Task<TItem> InsertOrUpdateInternalAsync(TItem item,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        _zoneTree.Upsert(item.Id, item);
        return _zoneTree.Get(item.Id)!;
    }

    protected override async Task<int> InsertOrUpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => items.Count(item => _zoneTree.InsertOrUpdate(item.Id, item)), cancellationToken);
    }

    protected override async Task<TItem?> GetInternalAsync(TId id, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _zoneTree.Get(id);
    }

    protected override async Task<long> CountInternalAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => _zoneTree.Count(), cancellationToken);
    }
}