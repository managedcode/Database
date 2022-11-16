using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.Core
{
    public abstract class BaseCollectionQueryable<TSource> : IOrderedCollectionQueryable<TSource>
    {
        protected readonly List<QueryItem> Predicates = new();

        protected enum QueryType
        {
            Where,
            OrderBy,
            OrderByDescending,
            ThenBy,
            ThenByDescending,
            Take,
            Skip,
        }

        protected struct QueryItem
        {
            public QueryType QueryType;
            public Expression<Func<TSource, object>> ExpressionObject;
            public Expression<Func<TSource, bool>> ExpressionBool;
            public int? Count;
        }

        public abstract IAsyncEnumerable<TSource> ToAsyncEnumerable(CancellationToken cancellationToken = default);
        public abstract Task<TSource?> FirstOrDefaultAsync(CancellationToken cancellationToken = default);
        public abstract Task<long> CountAsync(CancellationToken cancellationToken = default);
        public abstract Task<int> DeleteAsync(CancellationToken cancellationToken = default);

        public ICollectionQueryable<TSource> Where(Expression<Func<TSource, bool>> predicate)
        {
            Predicates.Add(new QueryItem { QueryType = QueryType.Where, ExpressionBool = predicate });
            return this;
        }

        public IOrderedCollectionQueryable<TSource> OrderBy(Expression<Func<TSource, object>> keySelector)
        {
            Predicates.Add(new QueryItem { QueryType = QueryType.OrderBy, ExpressionObject = keySelector });
            return this;
        }

        public IOrderedCollectionQueryable<TSource> OrderByDescending(Expression<Func<TSource, object>> keySelector)
        {
            Predicates.Add(new QueryItem { QueryType = QueryType.OrderByDescending, ExpressionObject = keySelector });
            return this;
        }


        public IOrderedCollectionQueryable<TSource> ThenBy(Expression<Func<TSource, object>> keySelector)
        {
            Predicates.Add(new QueryItem { QueryType = QueryType.ThenBy, ExpressionObject = keySelector });
            return this;
        }

        public IOrderedCollectionQueryable<TSource> ThenByDescending(Expression<Func<TSource, object>> keySelector)
        {
            Predicates.Add(new QueryItem { QueryType = QueryType.ThenByDescending, ExpressionObject = keySelector });
            return this;
        }

        public ICollectionQueryable<TSource> Take(int? count)
        {
            Predicates.Add(new QueryItem { QueryType = QueryType.Take, Count = count });
            return this;
        }

        public ICollectionQueryable<TSource> Skip(int count)
        {
            Predicates.Add(new QueryItem { QueryType = QueryType.Skip, Count = count });
            return this;
        }

        public async Task<List<TSource>> ToListAsync(CancellationToken cancellationToken = default)
        {
            return new List<TSource>(await ToAsyncEnumerable(cancellationToken).ToListAsync(cancellationToken));
        }
    }
}