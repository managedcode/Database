using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace ManagedCode.Database.MongoDB
{
    public class MongoDBCollectionQueryable<TItem> : BaseCollectionQueryable<TItem> where TItem : class, IItem<ObjectId>
    {
        private readonly IMongoCollection<TItem> _collection;

        public MongoDBCollectionQueryable(IMongoCollection<TItem> collection)
        {
            _collection = collection;
        }

        public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.Yield();

            foreach (var item in ApplyPredicates(Predicates))
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return item;
            }
        }

        public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
        {
            var query = ApplyPredicates(Predicates);

            return await Task.Run(() => query.FirstOrDefault(), cancellationToken);
        }

        public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            var query = ApplyPredicates(Predicates);

            return await Task.Run(() => query.LongCount(), cancellationToken);
        }

        public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
        {
            var ids =
                ApplyPredicates(Predicates)
                    .Select(d => d.Id);

            var filter = Builders<TItem>.Filter.In(d => d.Id, ids);

            var result = await _collection.DeleteManyAsync(filter, cancellationToken);

            return Convert.ToInt32(result.DeletedCount);
        }

        private IMongoQueryable<TItem> ApplyPredicates(IEnumerable<QueryItem> predicates)
        {
            var query = _collection.AsQueryable();

            foreach (var predicate in predicates)
            {
                query = predicate.QueryType switch
                {
                    QueryType.Where => query.Where(predicate.ExpressionBool),
                    QueryType.OrderBy => query.OrderBy(predicate.ExpressionObject),
                    QueryType.OrderByDescending => query.OrderByDescending(predicate.ExpressionObject),
                    QueryType.ThenBy => (query as IOrderedMongoQueryable<TItem>).ThenBy(predicate.ExpressionObject),
                    QueryType.ThenByDescending => (query as IOrderedMongoQueryable<TItem>)
                        .ThenByDescending(predicate.ExpressionObject),
                    QueryType.Take => predicate.Count.HasValue ? query.Take(predicate.Count.Value) : query,
                    QueryType.Skip => query.Skip(predicate.Count!.Value),
                    _ => query
                };
            }

            return query;
        }
    }
}