using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using ManagedCode.Database.Core;

namespace ManagedCode.Database.LiteDB;

public class LiteDbDBCollectionQueryable<TId, TItem> : BaseDBCollectionQueryable<TItem> where TItem : LiteDbItem<TId>, IItem<TId>, new()
{
    private readonly ILiteCollection<TItem> _collection;

    public LiteDbDBCollectionQueryable(ILiteCollection<TItem> collection)
    {
        _collection = collection;
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        var query = _collection.Query();

        foreach (var predicate in WherePredicates)
        {
            query = query.Where(predicate);
        }

        if (OrderByPredicates.Count > 0)
        {
            foreach (var predicate in OrderByPredicates)
            {
                query = query.OrderBy(predicate);
            }
        }

        if (OrderByDescendingPredicates.Count > 0)
        {
            foreach (var predicate in OrderByDescendingPredicates)
            {
                query = query.OrderByDescending(predicate);
            }
        }

        SkipValue ??= 0;

        if (TakeValue.HasValue)
        {
            foreach (var item in query.Skip(SkipValue.Value).Limit(TakeValue.Value).ToEnumerable())
            {
                yield return item;
            }
        }
        else
        {
            foreach (var item in query.Skip(SkipValue.Value).ToEnumerable())
            {
                yield return item;
            }
        }
    }

    public override async Task<long> LongCountAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        var query = _collection.Query();
        foreach (var predicate in WherePredicates)
        {
            query = query.Where(predicate);
        }

        return query.Count();
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _collection.DeleteMany(WherePredicates.First());
    }
}