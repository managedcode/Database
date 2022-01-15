using ManagedCode.Repository.Core;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Repository.EntityFramework
{
    public class EFRepository<TId, TItem, TContext> : EFBaseRepository<TId, TItem, TContext>
        where TItem : class, IEFItem<TId>, new()
        where TContext : DbContext
    {
        protected TContext _context { get; private set; }

        public EFRepository(TContext context)
        {
            _context = context;
        }

        protected override Task<int> CountAsyncInternal(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<int> CountAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> DeleteAsyncInternal(TId id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<int> DeleteAsyncInternal(IEnumerable<TId> ids, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override ValueTask DisposeAsyncInternal()
        {
            throw new NotImplementedException();
        }

        protected override void DisposeInternal()
        {
            throw new NotImplementedException();
        }

        protected override IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates, int? take = null, int skip = 0, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates, Expression<Func<TItem, object>> orderBy, Order orderType, int? take = null, int skip = 0, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override IAsyncEnumerable<TItem> FindAsyncInternal(IEnumerable<Expression<Func<TItem, bool>>> predicates, Expression<Func<TItem, object>> orderBy, Order orderType, Expression<Func<TItem, object>> thenBy, Order thenType, int? take = null, int skip = 0, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override IAsyncEnumerable<TItem> GetAllAsyncInternal(int? take = null, int skip = 0, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override IAsyncEnumerable<TItem> GetAllAsyncInternal(Expression<Func<TItem, object>> orderBy, Order orderType, int? take = null, int skip = 0, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<TItem> GetAsyncInternal(TId id, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<TItem> GetAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task InitializeAsyncInternal(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<TItem> InsertAsyncInternal(TItem item, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<TItem> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<TItem> UpdateAsyncInternal(TItem items, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        protected override Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }
    }
}
