using System.Linq.Expressions;
using ManagedCode.Database.Core;
using Microsoft.Extensions.Logging;
using Tenray.ZoneTree.Options;

namespace ManagedCode.ZoneTree.Cluster.DB;

public class ZoneTreeDBCollectionQueryable<TId, TItem> : BaseDBCollectionQueryable<TItem> where TItem : IItem<TId>
{
    private readonly ZoneTreeWrapper<TId, TItem> _zoneTree;

    public ZoneTreeDBCollectionQueryable(ZoneTreeWrapper<TId, TItem> zoneTree)
    {
        _zoneTree = zoneTree;
    }
    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
    {
        var query = _zoneTree.Enumerate();

        foreach (var predicate in WherePredicates)
        {
            query = query.Where(predicate.Compile());
        }

        if (OrderByPredicates.Count > 0)
        {
            foreach (var predicate in OrderByPredicates)
            {
                query = query.OrderBy(predicate.Compile());
            }
        }

        if (OrderByDescendingPredicates.Count > 0)
        {
            foreach (var predicate in OrderByDescendingPredicates)
            {
                query = query.OrderByDescending(predicate.Compile());
            }
        }

        SkipValue ??= 0;

        if (TakeValue.HasValue)
        {
            foreach (var item in query.Skip(SkipValue.Value).Take(TakeValue.Value))
            {
                yield return item;
            }
        }
        else
        {
            foreach (var item in query.Skip(SkipValue.Value))
            {
                yield return item;
            }
        }
        
    }

    public override Task<long> LongCountAsync(CancellationToken cancellationToken = default)
    {
        var conditions = WherePredicates.Select(s => s.Compile()).ToArray();
        return Task.FromResult(_zoneTree.Enumerate().Where(w => conditions.All(a => a.Invoke(w))).LongCount());
    }

    public override Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        var i = 0;
        var query = _zoneTree.Enumerate();
        
        foreach (var predicate in WherePredicates)
        {
            query = query.Where(predicate.Compile());
        }
        
        foreach (var item in query)
        {
            _zoneTree.Delete(item.Id);
            i++;
        }
        
        return Task.FromResult(i);
    }
}

public class ZoneTreeDBCollection<TId, TItem> : BaseDBCollection<TId, TItem> where TItem : IItem<TId>
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
    
    public override void Dispose()
    {
        _zoneTree.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        _zoneTree.Dispose();
        return ValueTask.CompletedTask;
    }

    protected override Task<TItem> InsertAsyncInternal(TItem item, CancellationToken token = default)
    {
        _zoneTree.Insert(item.Id, item);
        return Task.FromResult(item);
    }

    protected override Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var i = 0;
        foreach (var item in items)
        {
            i++;
            _zoneTree.Insert(item.Id, item);
        }

        return Task.FromResult(i);
    }

    protected override Task<TItem> UpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        _zoneTree.Update(item.Id, item);
        return Task.FromResult(item);
    }

    protected override Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var i = 0;
        foreach (var item in items)
        {
            _zoneTree.Update(item.Id, item);
            i++;
        }

        return Task.FromResult(i);
    }

    protected override Task<bool> DeleteAsyncInternal(TId id, CancellationToken token = default)
    {
        _zoneTree.Delete(id);
        return Task.FromResult(true);
    }

    protected override Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
    {
        _zoneTree.Delete(item.Id);
        return Task.FromResult(true);
    }

    protected override Task<int> DeleteAsyncInternal(IEnumerable<TId> ids, CancellationToken token = default)
    {
        var i = 0;
        foreach (var id in ids)
        {
            _zoneTree.Delete(id);
            i++;
        }

        return Task.FromResult(i);
    }

    protected override Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var i = 0;
        foreach (var item in items)
        {
            _zoneTree.Delete(item.Id);
            i++;
        }

        return Task.FromResult(i);
    }

    protected override Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
    {
        _zoneTree.DeleteAll();
        return Task.FromResult(true);
    }

    protected override Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        _zoneTree.Upsert(item.Id, item);
        return Task.FromResult(item);
    }

    protected override Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        var i = 0;
        foreach (var item in  items)
        {
            _zoneTree.Upsert(item.Id, item);
            i++;
        }
        
        return Task.FromResult(i);
    }

    protected override Task<TItem> GetAsyncInternal(TId id, CancellationToken token = default)
    {
        return Task.FromResult(_zoneTree.Get(id));
    }

    public override IDBCollectionQueryable<TItem> Query()
    {
        return new ZoneTreeDBCollectionQueryable<TId, TItem>(_zoneTree);
    }

    protected override Task<long> CountAsyncInternal(CancellationToken token = default)
    {
        return Task.FromResult(_zoneTree.Count());
    }
}