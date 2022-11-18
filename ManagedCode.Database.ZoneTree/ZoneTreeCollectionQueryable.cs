using ManagedCode.Database.Core;

namespace ManagedCode.Database.ZoneTree;

public class ZoneTreeCollectionQueryable<TId, TItem> : BaseCollectionQueryable<TItem> where TItem : IItem<TId>
{
    private readonly ZoneTreeWrapper<TId, TItem> _zoneTree;

    internal ZoneTreeCollectionQueryable(ZoneTreeWrapper<TId, TItem> zoneTree)
    {
        _zoneTree = zoneTree;
    }

    private IEnumerable<TItem> GetItemsInternal()
    {
        var zoneTreeQuery = _zoneTree.Enumerate();

        foreach (var query in Predicates)
            switch (query.QueryType)
            {
                case QueryType.Where:
                    zoneTreeQuery = zoneTreeQuery.Where(x => query.ExpressionBool.Compile().Invoke(x));
                    break;

                case QueryType.OrderBy:

                    // TODO: Maybe need to check is IOrderedQueryable and do throw exception
                    zoneTreeQuery = zoneTreeQuery.OrderBy(x => query.ExpressionObject.Compile().Invoke(x));
                    break;

                case QueryType.OrderByDescending:

                    // TODO: Maybe need to check is IOrderedQueryable and do throw exception
                    zoneTreeQuery = zoneTreeQuery.OrderByDescending(x => query.ExpressionObject.Compile().Invoke(x));
                    break;

                case QueryType.ThenBy:
                    if (zoneTreeQuery is IOrderedQueryable<TItem> orderedItems)
                    {
                        zoneTreeQuery = orderedItems.ThenBy(x => query.ExpressionObject.Compile().Invoke(x));
                        break;
                    }

                    throw new InvalidOperationException("Before ThenBy call first OrderBy.");

                case QueryType.ThenByDescending:
                    if (zoneTreeQuery is IOrderedQueryable<TItem> orderedDescendingItems)
                    {
                        zoneTreeQuery = orderedDescendingItems.ThenBy(x => query.ExpressionObject.Compile().Invoke(x));
                        break;
                    }

                    throw new InvalidOperationException("Before ThenBy call first OrderBy.");

                case QueryType.Take:
                    if (query.Count.HasValue) zoneTreeQuery = zoneTreeQuery.Take(query.Count.Value);
                    break;

                case QueryType.Skip:
                    if (query.Count.HasValue) zoneTreeQuery = zoneTreeQuery.Skip(query.Count.Value);
                    break;
            }

        return zoneTreeQuery;
    }


    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
    {
        foreach (var item in GetItemsInternal()) yield return item;
    }

    public override Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetItemsInternal().LongCount());
    }

    public override Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        var count = 0;

        foreach (var item in GetItemsInternal())
        {
            _zoneTree.Delete(item.Id);
            count++;
        }

        return Task.FromResult(count);
    }
}