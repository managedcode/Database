using ManagedCode.Database.Core;

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

    protected override Task<TItem> InsertInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        _zoneTree.Insert(item.Id, item);
        return Task.FromResult(item);
    }

    protected override Task<int> InsertInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var i = 0;
        foreach (var item in items)
        {
            i++;
            _zoneTree.Insert(item.Id, item);
        }

        return Task.FromResult(i);
    }

    protected override Task<TItem> UpdateInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        _zoneTree.Update(item.Id, item);
        return Task.FromResult(item);
    }

    protected override Task<int> UpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var i = 0;
        foreach (var item in items)
        {
            _zoneTree.Update(item.Id, item);
            i++;
        }

        return Task.FromResult(i);
    }

    protected override Task<bool> DeleteInternalAsync(TId id, CancellationToken cancellationToken = default)
    {
        _zoneTree.Delete(id);
        return Task.FromResult(true);
    }

    protected override Task<bool> DeleteInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        _zoneTree.Delete(item.Id);
        return Task.FromResult(true);
    }

    protected override Task<int> DeleteInternalAsync(IEnumerable<TId> ids,
        CancellationToken cancellationToken = default)
    {
        var i = 0;
        foreach (var id in ids)
        {
            _zoneTree.Delete(id);
            i++;
        }

        return Task.FromResult(i);
    }

    protected override Task<int> DeleteInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var i = 0;
        foreach (var item in items)
        {
            _zoneTree.Delete(item.Id);
            i++;
        }

        return Task.FromResult(i);
    }

    protected override Task<bool> DeleteCollectionInternalAsync(CancellationToken cancellationToken = default)
    {
        _zoneTree.DeleteAll();
        return Task.FromResult(true);
    }

    protected override Task<TItem> InsertOrUpdateInternalAsync(TItem item,
        CancellationToken cancellationToken = default)
    {
        _zoneTree.Upsert(item.Id, item);
        return Task.FromResult(item);
    }

    protected override Task<int> InsertOrUpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var i = 0;
        foreach (var item in items)
        {
            _zoneTree.Upsert(item.Id, item);
            i++;
        }

        return Task.FromResult(i);
    }

    protected override Task<TItem?> GetInternalAsync(TId id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_zoneTree.Get(id));
    }

    protected override Task<long> CountInternalAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_zoneTree.Count());
    }
}