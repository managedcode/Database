using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ManagedCode.Database.Core.InMemory;

public class InMemoryDBCollectionQueryable<TId, TItem> : BaseDBCollectionQueryable<TItem> where TId : notnull
{
    private readonly ConcurrentDictionary<TId, TItem> _storage;

    public InMemoryDBCollectionQueryable(ConcurrentDictionary<TId, TItem> storage)
    {
        _storage = storage;
    }

    private IEnumerable<KeyValuePair<TId, TItem>> GetItemsInternal()
    {
        var items = _storage.AsEnumerable();

        foreach (var query in Predicates)
        {
            switch(query.QueryType)
            {
                case QueryType.Where:
                    items = items.Where(x => query.ExpressionBool.Compile().Invoke(x.Value));
                    break;

                case QueryType.OrderBy:
                    items = items.OrderBy(x => query.ExpressionObject.Compile().Invoke(x.Value));
                    break;

                case QueryType.OrderByDescending:
                    items = items.OrderByDescending(x => query.ExpressionObject.Compile().Invoke(x.Value));
                    break;

                case QueryType.Take:
                    items = items.Take(query.Count.GetValueOrDefault());
                    break;

                case QueryType.Skip:
                    items = items.Skip(query.Count.GetValueOrDefault());
                    break;

                default: 
                    break;
            }
        }
            foreach (var item in items)
            {
                yield return item;
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

    public override Task<TItem> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    public override Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult((long)GetItemsInternal().Count());
    }

    public override Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        int count = 0;
        foreach (var item in GetItemsInternal())
        {
            count++;
            _storage.Remove(item.Key, out _);
        }

        return Task.FromResult(count);
    }
}