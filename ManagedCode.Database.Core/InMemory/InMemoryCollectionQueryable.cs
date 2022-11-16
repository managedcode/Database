using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ManagedCode.Database.Core.InMemory
{
    public class InMemoryCollectionQueryable<TId, TItem> : BaseCollectionQueryable<TItem> where TId : notnull
    {
        private readonly ConcurrentDictionary<TId, TItem> _storage;

        public InMemoryCollectionQueryable(ConcurrentDictionary<TId, TItem> storage)
        {
            _storage = storage;
        }

        private IEnumerable<KeyValuePair<TId, TItem>> GetItemsInternal()
        {
            IEnumerable<KeyValuePair<TId, TItem>> items = _storage.AsEnumerable();

            foreach (var query in Predicates)
            {
                switch(query.QueryType)
                {
                    case QueryType.Where:
                        items = items.Where(x => query.ExpressionBool.Compile().Invoke(x.Value));
                        break;

                    case QueryType.OrderBy:
                        if (items is IOrderedEnumerable<KeyValuePair<TId, TItem>>)
                        {
                            throw new InvalidOperationException("After OrderBy call ThenBy.");
                        }

                        items = items.OrderBy(x => query.ExpressionObject.Compile().Invoke(x.Value));
                        break;

                    case QueryType.OrderByDescending:
                        if (items is IOrderedEnumerable<KeyValuePair<TId, TItem>>)
                        {
                            throw new InvalidOperationException("After OrderBy call ThenBy.");
                        }

                        items = items.OrderByDescending(x => query.ExpressionObject.Compile().Invoke(x.Value));
                        break;

                    case QueryType.ThenBy:
                        if (items is IOrderedEnumerable<KeyValuePair<TId, TItem>> orderedItems)
                        {
                            items = orderedItems.ThenBy(x => query.ExpressionObject.Compile().Invoke(x.Value));
                            break;
                        }

                        throw new InvalidOperationException("Before ThenBy call first OrderBy.");

                    case QueryType.ThenByDescending:
                        if (items is IOrderedEnumerable<KeyValuePair<TId, TItem>> orderedDescendingItems)
                        {
                            items = orderedDescendingItems.ThenByDescending(x => query.ExpressionObject.Compile().Invoke(x.Value));
                            break;
                        }

                        throw new InvalidOperationException("Before ThenBy call first OrderBy.");

                    case QueryType.Take:
                        if (query.Count.HasValue)
                        {
                            items = items.Take(query.Count.Value);
                        }
                        break;

                    case QueryType.Skip:
                        if(query.Count.HasValue)
                        {
                            items = items.Skip(query.Count.Value);
                        }
                        break;

                    default: 
                        break;
                }
            }
       
            return items;

        }

        public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            foreach (var item in GetItemsInternal())
            {
                yield return item.Value;
            }
        }

        public override Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<TItem?>(GetItemsInternal().FirstOrDefault().Value);
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
                _storage.Remove(item.Key, out _);
                count++;
            }

            return Task.FromResult(count);
        }
    }
}