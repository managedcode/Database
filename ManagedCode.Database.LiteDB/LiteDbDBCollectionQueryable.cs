/*using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using ManagedCode.Database.Core;
using ManagedCode.Database.LiteDB.Extensions;

namespace ManagedCode.Database.LiteDB;

public class LiteDbDBCollectionQueryable<TId, TItem> : BaseDBCollectionQueryable<TItem>
    where TItem : LiteDbItem<TId>, IItem<TId>, new()
{
    private readonly ILiteCollection<TItem> _collection;

    public LiteDbDBCollectionQueryable(ILiteCollection<TItem> collection)
    {
        _collection = collection;
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        var enumerable = _collection.Query()
            .Where(WherePredicates)
            .OrderBy(OrderByPredicates)
            .OrderByDescending(OrderByDescendingPredicates)
            .Skip(SkipValue)
            .Take(TakeValue)
            .ToEnumerable();

        foreach (var item in enumerable)
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return item;
        }
    }

    public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        return _collection.Query()
            .Where(WherePredicates)
            .FirstOrDefault();
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        return _collection.Query()
            .Where(WherePredicates)
            .Count();
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        // TODO: check

        return _collection.DeleteMany(WherePredicates.First());
    }
}*/