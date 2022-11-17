using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using SQLite;

namespace ManagedCode.Database.SQLite;

public class SQLiteDatabaseCollection<TId, TItem> : IDatabaseCollection<TId, TItem>
    where TItem : class, IItem<TId>, new()
{
    private readonly SQLiteConnection _database;

    public SQLiteDatabaseCollection(SQLiteConnection database)
    {
        _database = database;
    }

    public ICollectionQueryable<TItem> Query => new SQLiteCollectionQueryable<TId, TItem>(_database);

    public ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    public void Dispose()
    {
    }

    #region Get

    public async Task<TItem?> GetAsync(TId id, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return ExceptionCatcher.Execute(() => _database.Find<TItem>(id));
    }

    #endregion

    #region Count

    public async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var task = Task.Run(() => _database.Table<TItem>().LongCount(), cancellationToken);
        return await ExceptionCatcher.ExecuteAsync(task);
    }

    #endregion

    #region Insert

    public async Task<TItem> InsertAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        return ExceptionCatcher.Execute(() =>
        {
            _database.Insert(item);
            return _database.Find<TItem>(item.Id);
        });
    }

    public async Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        var task = Task.Run(() => _database.InsertAll(items), cancellationToken);
        return await ExceptionCatcher.ExecuteAsync(task);
    }

    #endregion

    #region InsertOrUpdate

    public async Task<TItem> InsertOrUpdateAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        return ExceptionCatcher.Execute(() =>
        {
            _database.InsertOrReplace(item);
            return _database.Find<TItem>(item.Id);
        });
    }

    public async Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        var task = Task.Run(() => items.Sum(item => _database.InsertOrReplace(item)), cancellationToken);
        return await ExceptionCatcher.ExecuteAsync(task);
    }

    #endregion

    #region Update

    public async Task<TItem> UpdateAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        return ExceptionCatcher.Execute(() =>
        {
            _database.Update(item);
            return _database.Find<TItem>(item.Id);
        });
    }

    public async Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        var task = Task.Run(() => _database.UpdateAll(items), cancellationToken);
        return await ExceptionCatcher.ExecuteAsync(task);
    }

    #endregion

    #region Delete

    public async Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return ExceptionCatcher.Execute(() => _database.Delete<TItem>(id) != 0);
    }

    public async Task<bool> DeleteAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return ExceptionCatcher.Execute(() => _database.Delete<TItem>(item) != 0);
    }

    public async Task<int> DeleteAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default)
    {
        var task = Task.Run(() => ids.Sum(id => _database.Delete<TItem>(id)), cancellationToken);
        return await ExceptionCatcher.ExecuteAsync(task);
    }

    public async Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        var task = Task.Run(() => items.Sum(item => _database.Delete<TItem>(item.Id)), cancellationToken);
        return await ExceptionCatcher.ExecuteAsync(task);
    }

    public async Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default)
    {
        var task = Task.Run(() => _database.DeleteAll<TItem>() != 0, cancellationToken);
        return await ExceptionCatcher.ExecuteAsync(task);
    }

    #endregion
}