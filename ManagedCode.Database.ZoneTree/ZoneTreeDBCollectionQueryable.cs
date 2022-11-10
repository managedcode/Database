using ManagedCode.Database.Core;

namespace ManagedCode.ZoneTree.Cluster.DB;

public class ZoneTreeDBCollectionQueryable<TId, TItem> : OldBaseDBCollectionQueryable<TItem> where TItem : IItem<TId>
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

    public override Task<TItem> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<long> CountAsync(CancellationToken cancellationToken = default)
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