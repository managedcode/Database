using ManagedCode.Repository.Core;
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
        where TContext : DbContext
    {
        protected DbSet<TItem> _items;
        protected TContext _context { get; private set; }

        public EFRepository(TContext context)
        {
            _context = context;
        }

        protected override async Task InitializeAsyncInternal(CancellationToken token = default)
        {
            _items = _context.Set<TItem>();
            await _context.Database.EnsureCreatedAsync(token);

            IsInitialized = true;
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
            throw new NotImplementedException();
        }

        protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Update

        protected override async Task<TItem> UpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override async Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Delete

        protected override async Task<bool> DeleteAsyncInternal(TId id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override async Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TId> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override async Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override async Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Get

        protected override async Task<TItem> GetAsyncInternal(TId id, CancellationToken token = default)
        {
            return await _items.FindAsync(id);
        }

        protected override async Task<TItem> GetAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            Func<TItem, bool> func = predicate.Compile();

            return await _items.FirstOrDefaultAsync(x => func(x), token);
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

        protected override async IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override async IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        #endregion

        #region Count

        protected override async Task<int> CountAsyncInternal(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override async Task<int> CountAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
