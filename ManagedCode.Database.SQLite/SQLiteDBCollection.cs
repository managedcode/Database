using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using SQLite;

namespace ManagedCode.Database.SQLite;

public class SQLiteDBCollection<TId, TItem> : BaseDBCollection<TId, TItem> where TItem : class, IItem<TId>, new()
{
    private readonly SQLiteConnection _database;

    public SQLiteDBCollection(SQLiteConnection database)
    {
        _database = database;
    }

    public override ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    public override void Dispose()
    {
    }

    #region Get

    protected override async Task<TItem> GetAsyncInternal(TId id, CancellationToken token = default)
    {
        await Task.Yield();
        return _database.Find<TItem>(id);
    }

    #endregion

    #region Count

    protected override async Task<long> CountAsyncInternal(CancellationToken token = default)
    {
        await Task.Yield();
        return _database.Table<TItem>().Count();
    }

    #endregion

    public override IDBCollectionQueryable<TItem> Query()
    {
        return new SQLiteDBCollectionQueryable<TId, TItem>(_database);
    }

    #region Insert

    protected override async Task<TItem> InsertAsyncInternal(TItem item, CancellationToken token = default)
    {
        await Task.Yield();
        var v = _database.Insert(item);
        return item;
    }

    protected override async Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        await Task.Yield();
        return _database.InsertAll(items);
    }

    #endregion

    #region InsertOrUpdate

    protected override async Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        await Task.Yield();
        _database.InsertOrReplace(item);
        return item;
    }

    protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        await Task.Yield();
        var count = 0;
        foreach (var item in items)
        {
            count += _database.InsertOrReplace(item);
        }

        return count;
    }

    #endregion

    #region Update

    protected override async Task<TItem> UpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        await Task.Yield();
        _database.Update(item);
        return item;
    }

    protected override async Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        await Task.Yield();
        var count = 0;
        foreach (var item in items)
        {
            count += _database.Update(item);
        }

        return count;
    }

    #endregion

    #region Delete

    protected override async Task<bool> DeleteAsyncInternal(TId id, CancellationToken token = default)
    {
        await Task.Yield();
        return _database.Delete<TItem>(id) != 0;
    }

    protected override async Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
    {
        await Task.Yield();
        return _database.Delete(item) != 0;
    }

    protected override async Task<int> DeleteAsyncInternal(IEnumerable<TId> ids, CancellationToken token = default)
    {
        await Task.Yield();
        var count = 0;
        foreach (var id in ids)
        {
            count += _database.Delete<TItem>(id);
        }

        return count;
    }

    protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        await Task.Yield();
        var count = 0;
        foreach (var item in items)
        {
            count += _database.Delete<TItem>(item.Id);
        }

        return count;
    }

    protected override async Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
    {
        await Task.Yield();
        return _database.DeleteAll<TItem>() != 0;
    }

    #endregion
}