using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using ManagedCode.Database.Core;

namespace ManagedCode.Database.LiteDB;

public class LiteDbDBCollection<TId, TItem> : BaseDBCollection<TId, TItem>
    where TItem : LiteDbItem<TId>, IItem<TId>, new()
{
    private readonly ILiteCollection<TItem> _collection;

    public LiteDbDBCollection(ILiteCollection<TItem> collection)
    {
        _collection = collection;
    }


    private ILiteCollection<TItem> GetDatabase()
    {
        return _collection;
    }

    public override ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    public override void Dispose()
    {
    }

    #region Insert
    
    protected override async Task<TItem> InsertAsyncInternal(TItem item, CancellationToken token = default)
    {
        await Task.Yield();
        var v = GetDatabase().Insert(item);
        return GetDatabase().FindById(v);
    }

    protected override async Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        await Task.Yield();
        return GetDatabase().InsertBulk(items);
    }

    #endregion

    #region InsertOrUpdate

    protected override async Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        await Task.Yield();
        GetDatabase().Upsert(item);
        return GetDatabase().FindById(new BsonValue(item.Id));
    }

    protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        await Task.Yield();
        return GetDatabase().Upsert(items);
    }

    #endregion

    #region Update

    protected override async Task<TItem> UpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        await Task.Yield();
        if (GetDatabase().Update(item))
        {
            return GetDatabase().FindById(new BsonValue(item.Id));
        }

        return default;
    }

    protected override async Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        await Task.Yield();
        return GetDatabase().Update(items);
    }

    #endregion

    #region Delete

    protected override async Task<bool> DeleteAsyncInternal(TId id, CancellationToken token = default)
    {
        await Task.Yield();
        return GetDatabase().Delete(new BsonValue(id));
    }

    protected override async Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
    {
        await Task.Yield();
        return GetDatabase().Delete(new BsonValue(item.Id));
    }

    protected override async Task<int> DeleteAsyncInternal(IEnumerable<TId> ids, CancellationToken token = default)
    {
        await Task.Yield();
        var count = 0;
        var db = GetDatabase();
        foreach (var id in ids)
        {
            if (db.Delete(new BsonValue(id)))
            {
                count++;
            }
        }

        return count;
    }

    protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        await Task.Yield();
        var count = 0;
        var db = GetDatabase();
        foreach (var item in items)
        {
            if (db.Delete(new BsonValue(item.Id)))
            {
                count++;
            }
        }

        return count;
    }

    protected override async Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
    {
        await Task.Yield();
        return GetDatabase().DeleteAll() > 0;
    }

    #endregion

    #region Get

    protected override async Task<TItem> GetAsyncInternal(TId id, CancellationToken token = default)
    {
        await Task.Yield();
        return GetDatabase().FindById(new BsonValue(id));
    }
    

    

    #endregion
    

    #region Count

    protected override async Task<long> CountAsyncInternal(CancellationToken token = default)
    {
        await Task.Yield();
        return GetDatabase().Count();
    }
    
    #endregion
    
    public override IDBCollectionQueryable<TItem> Query()
    {
        return new LiteDbDBCollectionQueryable<TId, TItem>(GetDatabase());
    }
}