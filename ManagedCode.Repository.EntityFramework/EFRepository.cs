using ManagedCode.Repository.Core;
using ManagedCode.Repository.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Repository.EntityFramework
{
    public class EFRepository<TId, TItem, TContext> : EFBaseRepository<TId, TItem, TContext>
        where TItem : class, IEFItem<TId>, new()
        where TContext : EFDbContext<TContext>
    {
        protected DbSet<TItem> _items;
        protected TContext _context { get; private set; }

        public EFRepository(TContext context)
        {
            _context = context;
            _items = _context.Set<TItem>();
            _context.Database.EnsureCreated();

            IsInitialized = true;
        }

        protected override Task InitializeAsyncInternal(CancellationToken token = default)
        {
            return Task.CompletedTask;
        }

        protected override ValueTask DisposeAsyncInternal()
        {
            return default;
        }

        protected override void DisposeInternal()
        {
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
            await _context.SaveChangesAsync(token);

            return 1;
        }

        #endregion

        #region InsertOrUpdate

        protected override async Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            var dbItem = GetAsync(item.Id);

            if (dbItem == null)
            {
                await InsertAsync(item);
            }
            else
            {
                await UpdateAsync(item);
            }

            return item;
        }

        protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            foreach (var item in items)
            {
                await InsertOrUpdateAsync(item);
            }

            return items.ToList().Count();
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
            var item = await GetAsync(id);
            _items.Remove(item);

            await _context.SaveChangesAsync();

            return true;
        }

        protected override async Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
        {
            _items.Remove(item);
            await _context.SaveChangesAsync();

            return true;
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TId> ids, CancellationToken token = default)
        {
            var itemsToDelete = ids.Select(id => new TItem { Id = id });

            _items.RemoveRange(itemsToDelete);
            await _context.SaveChangesAsync();

            return itemsToDelete.Count();
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> itemsToDelete, CancellationToken token = default)
        {
            _items.RemoveRange(itemsToDelete);
            await _context.SaveChangesAsync();

            return itemsToDelete.Count();
        }

        protected override async Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            var itemsToDelete = _items.Where(predicate);
            int count = itemsToDelete.Count();

            _items.RemoveRange(itemsToDelete);
            await _context.SaveChangesAsync();

            return count;
        }

        protected override async Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
        {
            var itemsToDelete = new List<TItem>();

            await foreach(var item in GetAllAsync())
            {
                itemsToDelete.Add(item);
            }

            _items.RemoveRange(itemsToDelete);
            await _context.SaveChangesAsync();

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
            int skip = 0, [EnumeratorCancellation] CancellationToken token = default)
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
                foreach (var item in query.Take(take.Value))
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    yield return item;
                }
            }
            else
            {
                foreach (var item in query)
                {
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    yield return item;
                }
            }
        }

        #endregion

        #region Find

        protected override IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            Expression<Func<TItem, object>> thenBy,
            Order thenType,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Count

        protected override async Task<int> CountAsyncInternal(CancellationToken token = default)
        {
            return await _items.CountAsync();
        }

        protected override async Task<int> CountAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates, CancellationToken token = default)
        {
            int count = 0;

            foreach (var predicate in predicates)
            {
                count += await _items.Where(predicate).CountAsync();
            }

            return count;
        }

        #endregion
    }
}
