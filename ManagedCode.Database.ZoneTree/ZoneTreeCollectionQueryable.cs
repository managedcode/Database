using System.Runtime.CompilerServices;
using ManagedCode.Database.Core;

namespace ManagedCode.Database.ZoneTree;

public class ZoneTreeCollectionQueryable<TId, TItem> : BaseCollectionQueryable<TItem> where TItem : IItem<TId>
{
    private readonly ZoneTreeWrapper<TId, TItem> _zoneTree;

    internal ZoneTreeCollectionQueryable(ZoneTreeWrapper<TId, TItem> zoneTree)
    {
        _zoneTree = zoneTree;
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        foreach (var item in ApplyPredicates())
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return item;
        }
    }

    public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        return ApplyPredicates().FirstOrDefault();
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var query = ApplyPredicates();

        return await Task.Run(() => query.LongCount(), cancellationToken);
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        var count = 0;

        foreach (var item in ApplyPredicates())
        {
            cancellationToken.ThrowIfCancellationRequested();

            _zoneTree.Delete(item.Id);
            count++;
        }

        return count;
    }

    private IEnumerable<TItem> ApplyPredicates()
    {
        var enumerable = _zoneTree.Enumerate();

        foreach (var predicate in Predicates)
            enumerable = predicate.QueryType switch
            {
                QueryType.Where => enumerable.Where(x => predicate.ExpressionBool.Compile().Invoke(x)),
                QueryType.OrderBy => enumerable
                    .OrderBy(x => predicate.ExpressionObject.Compile().Invoke(x)),
                QueryType.OrderByDescending => enumerable
                    .OrderByDescending(x => predicate.ExpressionObject.Compile().Invoke(x)),
                QueryType.ThenBy => (enumerable as IOrderedQueryable<TItem>)!
                    .ThenBy(x => predicate.ExpressionObject.Compile().Invoke(x)),
                QueryType.ThenByDescending => (enumerable as IOrderedQueryable<TItem>)!
                    .ThenByDescending(x => predicate.ExpressionObject.Compile().Invoke(x)),
                QueryType.Take => predicate.Count.HasValue ? enumerable.Take(predicate.Count.Value) : enumerable,
                QueryType.Skip => enumerable.Skip(predicate.Count!.Value),
                _ => enumerable
            };

        return enumerable;
    }
}