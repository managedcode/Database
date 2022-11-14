using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using SQLite;

namespace ManagedCode.Database.SQLite;

public class SQLiteDBCollectionQueryable<TId, TItem> : BaseDBCollectionQueryable<TItem> 
    where TItem : class, IItem<TId>, new()
{
    private readonly SQLiteConnection _connection;

    public SQLiteDBCollectionQueryable(SQLiteConnection connection)
    {
        _connection = connection;
    }

    private IEnumerable<TItem> GetItemsInternal()
    {
        IEnumerable<TItem> items = _connection.Table<TItem>();

        foreach (var query in Predicates)
        {
            switch (query.QueryType)
            {
                case QueryType.Where:
                    items = items.Where(x => query.ExpressionBool.Compile().Invoke(x));
                    break;

                case QueryType.OrderBy:
                    if (items is IOrderedEnumerable<TItem>)
                    {
                        throw new InvalidOperationException("After OrderBy call ThenBy.");
                    }
                    items = items.OrderBy(x => query.ExpressionObject.Compile().Invoke(x));
                    break;

                case QueryType.OrderByDescending:
                    if (items is IOrderedEnumerable<TItem>)
                    {
                        throw new InvalidOperationException("After OrderBy call ThenBy.");

                    }
                    items = items.OrderByDescending(x => query.ExpressionObject.Compile().Invoke(x));
                    break;

                case QueryType.ThenBy:
                    if (items is IOrderedEnumerable<TItem> orderedItems)
                    {
                        items = orderedItems.ThenBy(x => query.ExpressionObject.Compile().Invoke(x));
                        break;
                    }
                    throw new InvalidOperationException("Before ThenBy call first OrderBy.");

                case QueryType.ThenByDescending:
                    if (items is IOrderedEnumerable<TItem> orderedDescendingItems)
                    {
                        items = orderedDescendingItems.ThenByDescending(x => query.ExpressionObject.Compile().Invoke(x));
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
                    if (query.Count.HasValue)
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
            yield return item;
        }

    }

    public override Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(GetItemsInternal().FirstOrDefault());
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        return GetItemsInternal().Count();
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        int count = 0;

        foreach (var item in GetItemsInternal())
        {
            _connection.Delete<TItem>(item);
            count++;
        }

        return count;
    }
}