using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using SQLite;

namespace ManagedCode.Database.SQLite;

public class SQLiteDBCollectionQueryable<TId, TItem> : BaseDBCollectionQueryable<TItem> where TItem : class, IItem<TId>, new()
{
    private readonly SQLiteConnection _connection;

    public SQLiteDBCollectionQueryable(SQLiteConnection connection)
    {
        _connection = connection;
    }

    private IEnumerable<KeyValuePair<TId, TItem>> GetItemsInternal()
    {
        var items = _connection.Table<KeyValuePair<TId, TItem>>().AsEnumerable();

        foreach (var query in Predicates)
        {
            switch (query.QueryType)
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