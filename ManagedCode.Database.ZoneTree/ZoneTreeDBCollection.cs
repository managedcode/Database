using ManagedCode.Database.Core;
using Microsoft.Extensions.Logging;
using Tenray.ZoneTree.Options;

namespace ManagedCode.ZoneTree.Cluster.DB;

public class ZoneTreeDBCollection<TId, TItem> : IDBCollection<TId, TItem> where TItem : IItem<TId>
{
    private readonly ZoneTreeWrapper<TId, TItem> _zoneTree;

    public ZoneTreeDBCollection(ILogger logger, string path)
    {
        _zoneTree = new ZoneTreeWrapper<TId, TItem>(logger, path);
        _zoneTree.Open(new ZoneTreeOptions<TId, TItem?>()
        {
            Path = path,
            WALMode = WriteAheadLogMode.Sync,
            DiskSegmentMode = DiskSegmentMode.SingleDiskSegment,
            StorageType = StorageType.File,
            ValueSerializer = new JsonSerializer<TItem?>()
        });
    }

    public IDBCollectionQueryable<TItem> Query => new ZoneTreeDBCollectionQueryable<TId, TItem>(_zoneTree);


    public void Dispose()
    {
        _zoneTree.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        _zoneTree.Dispose();
        return ValueTask.CompletedTask;
    }

    public Task<TItem> InsertAsync(TItem item, CancellationToken token = default)
    {
        _zoneTree.Insert(item.Id, item);
        return Task.FromResult(item);
    }

    public Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var i = 0;
        foreach (var item in items)
        {
            i++;
            _zoneTree.Insert(item.Id, item);
        }

        return Task.FromResult(i);
    }

    public Task<TItem> UpdateAsync(TItem item, CancellationToken token = default)
    {
        _zoneTree.Update(item.Id, item);
        return Task.FromResult(item);
    }

    public Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var i = 0;
        foreach (var item in items)
        {
            _zoneTree.Update(item.Id, item);
            i++;
        }

        return Task.FromResult(i);
    }

    public Task<bool> DeleteAsync(TId id, CancellationToken token = default)
    {
        _zoneTree.Delete(id);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(TItem item, CancellationToken token = default)
    {
        _zoneTree.Delete(item.Id);
        return Task.FromResult(true);
    }

    public Task<int> DeleteAsync(IEnumerable<TId> ids, CancellationToken token = default)
    {
        var i = 0;
        foreach (var id in ids)
        {
            _zoneTree.Delete(id);
            i++;
        }

        return Task.FromResult(i);
    }

    public Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var i = 0;
        foreach (var item in items)
        {
            _zoneTree.Delete(item.Id);
            i++;
        }

        return Task.FromResult(i);
    }

    public Task<bool> DeleteAllAsync(CancellationToken token = default)
    {
        _zoneTree.DeleteAll();
        return Task.FromResult(true);
    }

    public Task<TItem> InsertOrUpdateAsync(TItem item, CancellationToken token = default)
    {
        _zoneTree.Upsert(item.Id, item);
        return Task.FromResult(item);
    }

    public Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var i = 0;
        foreach (var item in items)
        {
            _zoneTree.Upsert(item.Id, item);
            i++;
        }

        return Task.FromResult(i);
    }

    public Task<TItem> GetAsync(TId id, CancellationToken token = default)
    {
        return Task.FromResult(_zoneTree.Get(id));
    }

    public Task<long> CountAsync(CancellationToken token = default)
    {
        return Task.FromResult(_zoneTree.Count());
    }
}