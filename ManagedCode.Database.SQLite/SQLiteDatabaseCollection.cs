using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Exceptions;
using SQLite;

namespace ManagedCode.Database.SQLite;

public class SQLiteDatabaseCollection<TId, TItem> : BaseDatabaseCollection<TId, TItem>
    where TItem : class, IItem<TId>, new()
{
    private readonly SQLiteConnection _database;

    public SQLiteDatabaseCollection(SQLiteConnection database)
    {
        _database = database;
    }

    public override ICollectionQueryable<TItem> Query => new SQLiteCollectionQueryable<TId, TItem>(_database);

    public override ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    public override void Dispose()
    {
    }

    #region Get

    protected override async Task<TItem?> GetInternalAsync(TId id, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _database.Find<TItem>(id);
    }

    #endregion

    #region Count

    protected override async Task<long> CountInternalAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => _database.Table<TItem>().LongCount(), cancellationToken);
    }

    #endregion

    #region Insert

    protected override async Task<TItem> InsertInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        _database.Insert(item);
        return _database.Find<TItem>(item.Id);
    }

    protected override async Task<int> InsertInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => _database.InsertAll(items), cancellationToken);
    }

    #endregion

    #region InsertOrUpdate

    protected override async Task<TItem> InsertOrUpdateInternalAsync(TItem item,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        _database.InsertOrReplace(item);
        return _database.Find<TItem>(item.Id);
    }

    protected override async Task<int> InsertOrUpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => items.Sum(item => _database.InsertOrReplace(item)), cancellationToken);
    }

    #endregion

    #region Update

    protected override async Task<TItem> UpdateInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        var count = _database.Update(item);

        if (count == 0)
        {
            throw new DatabaseException("Entity not found in collection.");
        }

        return _database.Find<TItem>(item.Id);
    }

    protected override async Task<int> UpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => _database.UpdateAll(items), cancellationToken);
    }

    #endregion

    #region Delete

    protected override async Task<bool> DeleteInternalAsync(TId id, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _database.Delete<TItem>(id) != 0;
    }

    protected override async Task<bool> DeleteInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        return _database.Delete<TItem>(item) != 0;
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<TId> ids,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => ids.Sum(id => _database.Delete<TItem>(id)), cancellationToken);
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => items.Sum(item => _database.Delete<TItem>(item.Id)), cancellationToken);
    }

    protected override async Task<bool> DeleteCollectionInternalAsync(CancellationToken cancellationToken = default)
    {
        await Task.Run(() => _database.DeleteAll<TItem>(), cancellationToken);

        return await CountInternalAsync(cancellationToken) == 0;
    }

    #endregion
}