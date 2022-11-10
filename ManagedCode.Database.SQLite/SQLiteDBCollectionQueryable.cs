using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using SQLite;

namespace ManagedCode.Database.SQLite;

public class SQLiteDBCollectionQueryable<TId, TItem> : OldBaseDBCollectionQueryable<TItem> where TItem : class, IItem<TId>, new()
{
    private readonly SQLiteConnection _connection;

    public SQLiteDBCollectionQueryable(SQLiteConnection connection)
    {
        _connection = connection;
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        var query = _connection.Table<TItem>();

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
            foreach (var item in query.Skip(SkipValue.Value).Take(TakeValue.Value))
            {
                yield return item;
            }
        }
        else
        {
            foreach (var item in query.Skip(SkipValue.Value))
            {
                yield return item;
            }
        }
    }

    public override Task<TItem> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        var query = _connection.Table<TItem>();
        foreach (var predicate in WherePredicates)
        {
            query = query.Where(predicate);
        }

        return query.Count();
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        var ids = _connection.Table<TItem>().Where(WherePredicates.FirstOrDefault()).Select(s => s.Id);
        var count = 0;
        foreach (var id in ids)
        {
            _connection.Delete<TItem>(id);
            count++;
        }

        return count;
    }
}