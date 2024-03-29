﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using ManagedCode.Database.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ManagedCode.Database.EntityFramework;


public class EFDatabase : BaseDatabase, IDatabase<DbContext>
{
    private readonly DbContext _context;

    public EFDatabase(DbContext context)
    {
        _context = context;
        IsInitialized = true;
    }

    
    protected override ValueTask DisposeAsyncInternal()
    {
        return DataBase.DisposeAsync();
    }

    protected override void DisposeInternal()
    {
        DataBase.Dispose();
    }
    
    protected override async Task InitializeAsyncInternal(CancellationToken token = default)
    {
        await _context.Database.EnsureCreatedAsync(token);
    }
    
    public override Task Delete(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public DbContext DataBase { get; }
    
    public EFDBCollection<TId, TItem, TContext> GetCollection<TId, TItem, TContext>()     
        where TItem : class, IEFItem<TId>, new()
        where TContext : EFDbContext<TContext>
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }
        
        return new  EFDBCollection<TId, TItem, TContext>(DataBase);
    }
}

public class EFDBCollection<TId, TItem, TContext> : EFBaseDBCollection<TId, TItem, TContext>
    where TItem : class, IEFItem<TId>, new()
    where TContext : EFDbContext<TContext>
{
    protected DbSet<TItem> _items;

    public EFDBCollection(TContext context)
    {
        _context = context;

        InitializeAsyncInternal().Wait();
    }

    protected TContext _context { get; }

    protected override async Task InitializeAsyncInternal(CancellationToken token = default)
    {
        _items = _context.Set<TItem>();


        IsInitialized = true;
    }

    public override void Dispose()
    {
        _context.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        return _context.DisposeAsync();
    }

    #region Insert

    protected override async Task<TItem> InsertAsyncInternal(TItem item, CancellationToken token = default)
    {
        await _items.AddAsync(item, token);
        await _context.SaveChangesAsync(token);

        return item;
    }

    protected override async Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        await _items.AddRangeAsync(items, token);
        return await _context.SaveChangesAsync(token);
    }

    #endregion

    #region InsertOrUpdate

    protected override async Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        var dbItem = GetAsync(item.Id, token);
        
        if (dbItem == null)
        {
            await InsertAsync(item, token);
        }
        else
        {
            await UpdateAsync(item, token);
        }

        return item;
    }

    protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        foreach (var item in items)
        {
            await InsertOrUpdateAsync(item, token);
        }

        return items.Count();
    }

    #endregion

    #region Update

    protected override async Task<TItem> UpdateAsyncInternal(TItem item, CancellationToken token = default)
    {
        _items.Update(item);
        await _context.SaveChangesAsync(token);

        return item;
    }

    protected override async Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
    {
        _items.UpdateRange(items);
        await _context.SaveChangesAsync(token);

        return 1;
    }

    #endregion

    #region Delete

    protected override async Task<bool> DeleteAsyncInternal(TId id, CancellationToken token = default)
    {
        var item = new TItem { Id = id };

        _context.Entry(item).State = EntityState.Deleted;
        await _context.SaveChangesAsync(token);

        return true;
    }

    protected override async Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
    {
        _context.Entry(item).State = EntityState.Deleted;
        await _context.SaveChangesAsync(token);

        return true;
    }

    protected override async Task<int> DeleteAsyncInternal(IEnumerable<TId> ids, CancellationToken token = default)
    {
        var itemsToDelete = ids.Select(id => new TItem { Id = id });

        foreach (var item in itemsToDelete)
        {
            _context.Entry(item).State = EntityState.Deleted;
        }

        await _context.SaveChangesAsync(token);

        return itemsToDelete.Count();
    }

    protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> itemsToDelete, CancellationToken token = default)
    {
        foreach (var item in itemsToDelete)
        {
            _context.Entry(item).State = EntityState.Deleted;
        }

        await _context.SaveChangesAsync(token);

        return itemsToDelete.Count();
    }

    protected override async Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
    {
        var itemsToDelete = _items.Where(predicate);
        var count = itemsToDelete.Count();

        foreach (var item in itemsToDelete)
        {
            _context.Entry(item).State = EntityState.Deleted;
        }

        await _context.SaveChangesAsync(token);

        return count;
    }

    protected override async Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
    {
        await foreach (var item in GetAllAsync())
        {
            _context.Entry(item).State = EntityState.Deleted;
        }

        await _context.SaveChangesAsync(token);

        return true;
    }

    #endregion

    #region Get

    protected override async Task<TItem> GetAsyncInternal(TId id, CancellationToken token = default)
    {
        return await _items.FindAsync(id);
    }

    protected override async Task<TItem> GetAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
    {
        return await _items.FirstOrDefaultAsync(predicate, token);
    }

    protected override async IAsyncEnumerable<TItem> GetAllAsyncInternal(int? take = null,
        int skip = 0,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        await Task.Yield();
        var query = _items.Skip(skip);

        if (take != null)
        {
            query = query.Take(take.Value);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        foreach (var item in query)
        {
            if (token.IsCancellationRequested)
            {
                break;
            }

            yield return item;
        }
    }

    protected override async IAsyncEnumerable<TItem> GetAllAsyncInternal(Expression<Func<TItem, object>> orderBy,
        Order orderType,
        int? take = null,
        int skip = 0,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        await Task.Yield();
        var query = _items.Skip(skip);

        if (take != null)
        {
            query = query.Take(take.Value);
        }

        if (orderType == Order.By)
        {
            query = query.OrderBy(orderBy);
        }
        else
        {
            query.OrderByDescending(orderBy);
        }

        if (take != null)
        {
            query = query.Take(take.Value);
        }

        foreach (var item in query)
        {
            if (token.IsCancellationRequested)
            {
                break;
            }

            yield return item;
        }
    }

    #endregion

    #region Find

    protected override async IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates,
        int? take = null,
        int skip = 0,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        await Task.Yield();

        var query = _items.AsQueryable();

        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        query = query.Skip(skip);

        if (take != null)
        {
            query = query.Take(take.Value);
        }

        if (take.HasValue)
        {
            query = query.Take(take.Value);
        }

        var list = query.ToList();

        foreach (var item in list)
        {
            if (token.IsCancellationRequested)
            {
                break;
            }

            yield return item;
        }
    }

    protected override async IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates,
        Expression<Func<TItem, object>> orderBy,
        Order orderType,
        int? take = null,
        int skip = 0,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        await Task.Yield();

        var query = _items.AsQueryable();

        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        query = query.Skip(skip);

        if (take != null)
        {
            query = query.Take(take.Value);
        }

        if (orderType == Order.By)
        {
            query = query.OrderBy(orderBy);
        }
        else
        {
            query.OrderByDescending(orderBy);
        }

        var list = query.ToList();

        foreach (var item in list)
        {
            if (token.IsCancellationRequested)
            {
                break;
            }

            yield return item;
        }
    }

    protected override async IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates,
        Expression<Func<TItem, object>> orderBy,
        Order orderType,
        Expression<Func<TItem, object>> thenBy,
        Order thenType,
        int? take = null,
        int skip = 0,
        CancellationToken token = default)
    {
        await Task.Yield();
        var query = _items.AsQueryable();

        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        if (orderType == Order.By)
        {
            query = query.OrderBy(orderBy);
        }
        else
        {
            query.OrderByDescending(orderBy);
        }

        if (thenType == Order.By)
        {
            query = query.OrderBy(thenBy);
        }
        else
        {
            query.OrderByDescending(thenBy);
        }

        query = query.Skip(skip);

        if (take != null)
        {
            query = query.Take(take.Value);
        }

        var list = query.ToList();

        foreach (var item in list)
        {
            yield return item;
        }
    }

    #endregion

    #region Count

    protected override async Task<int> CountAsyncInternal(CancellationToken token = default)
    {
        return await _items.CountAsync();
    }

    protected override async Task<int> CountAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates, CancellationToken token = default)
    {
        var query = _items.AsQueryable();

        foreach (var predicate in predicates)
        {
            query = query.Where(predicate);
        }

        return await query.CountAsync();
    }

    #endregion
}