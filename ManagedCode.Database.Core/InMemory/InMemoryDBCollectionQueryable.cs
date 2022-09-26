using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.Core.InMemory;

public class InMemoryDBCollectionQueryable<TId, TItem> : BaseDBCollectionQueryable<TItem>
{
    private readonly Dictionary<TId, TItem> _storage;

    public InMemoryDBCollectionQueryable(Dictionary<TId, TItem> storage)
    {
        _storage = storage;
    }

    private IEnumerable<KeyValuePair<TId, TItem>> GetItemsInternal()
    {
        List<TItem> list;
        lock (_storage)
        {
            IEnumerable<KeyValuePair<TId,TItem>> items = null;

            foreach (var predicate in WherePredicates)
            {
                if (items == null)
                {
                    items = _storage.Where(x=>predicate.Compile().Invoke(x.Value));
                }
                else
                {
                    items = items.Where(x=>predicate.Compile().Invoke(x.Value));
                }
            }


            if (OrderByPredicates.Count > 0 || OrderByDescendingPredicates.Count > 0)
            {
                IOrderedEnumerable<KeyValuePair<TId,TItem>> orderedItems = null;
                bool firstOrderBy = true;
                foreach (var predicate in OrderByPredicates)
                {
                    if (firstOrderBy)
                    {
                        orderedItems = items.OrderBy(x=>predicate.Compile().Invoke(x.Value));
                        firstOrderBy = false;
                    }
                    else
                    {
                        orderedItems = orderedItems.ThenBy(x=>predicate.Compile().Invoke(x.Value));
                    }
                }

                bool firstOrderByDescending = true;
                foreach (var predicate in OrderByDescendingPredicates)
                {
                    if (firstOrderByDescending)
                    {
                        orderedItems = items.OrderByDescending(x=>predicate.Compile().Invoke(x.Value));
                        firstOrderByDescending = false;
                    }
                    else
                    {
                        orderedItems = orderedItems.ThenByDescending(x=>predicate.Compile().Invoke(x.Value));
                    }
                }

                items = orderedItems;
            }
            

            if (SkipValue.HasValue)
            {
                items = items.Skip(SkipValue.Value);
            }

            if (TakeValue.HasValue)
            {
                items = items.Take(TakeValue.Value);
            }

            foreach (var item in items)
            {
                yield return item;
            }
        }
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        foreach (var item in GetItemsInternal())
        {
            yield return item.Value;
        }
    }

    public override Task<long> LongCountAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((long)GetItemsInternal().Count());
    }

    public override Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        int count = 0;
        foreach (var item in GetItemsInternal())
        {
            count++;
            _storage.Remove(item.Key);
        }

        return Task.FromResult(count);
    }
}
