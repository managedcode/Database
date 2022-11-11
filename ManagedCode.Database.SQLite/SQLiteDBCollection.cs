using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using SQLite;

namespace ManagedCode.Database.SQLite;

public class SQLiteDBCollection<TId, TItem> : IDBCollection<TId, TItem> where TItem : class, IItem<TId>, new()
{
    private readonly SQLiteConnection _database;

    public SQLiteDBCollection(SQLiteConnection database)
    {
        _database = database;
    }

    public IDBCollectionQueryable<TItem> Query => new SQLiteDBCollectionQueryable<TId, TItem>(_database);

    public ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    public void Dispose()
    {
    }

    #region Get

    public async Task<TItem> GetAsync(TId id, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _database.Find<TItem>(id);
    }

    #endregion

    #region Count

    public async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _database.Table<TItem>().Count();
    }

    #endregion

    #region Insert

    public async Task<TItem> InsertAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        var v = _database.Insert(item);
        return item;
    }

    public async Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _database.InsertAll(items);
    }

    #endregion

    #region InsertOrUpdate

    public async Task<TItem> InsertOrUpdateAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        _database.InsertOrReplace(item);
        return item;
    }

    public async Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
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

    public async Task<TItem> UpdateAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        _database.Update(item);
        return item;
    }

    public async Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
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

    public async Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _database.Delete<TItem>(id) != 0;
    }

    public async Task<bool> DeleteAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _database.Delete(item) != 0;
    }

    public async Task<int> DeleteAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        var count = 0;
        foreach (var id in ids)
        {
            count += _database.Delete<TItem>(id);
        }

        return count;
    }

    public async Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        var count = 0;
        foreach (var item in items)
        {
            count += _database.Delete<TItem>(item.Id);
        }

        return count;
    }

    public async Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _database.DeleteAll<TItem>() != 0;
    }

    #endregion
}