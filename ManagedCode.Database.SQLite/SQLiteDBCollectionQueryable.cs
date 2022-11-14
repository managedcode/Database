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

    private IEnumerable<KeyValuePair<TId, TItem>> GetItemsInternal()
    {
        IEnumerable<KeyValuePair<TId, TItem>> items = _connection.Table<KeyValuePair<TId, TItem>>();

        foreach (var query in Predicates)
        {
            switch (query.QueryType)
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
            yield return item.Value;
        }

    }

    public override Task<TItem> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        int count = 0;

        foreach (var item in GetItemsInternal())
        {
            count++;
        }

        return count;
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        int count = 0;

        foreach (var item in GetItemsInternal())
        {
            _connection.Delete<TItem>(item.Key);
            count++;
        }

        return count;
    }
}