using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using ManagedCode.Database.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.DynamoDB
{
    public class DynamoDBCollectionQueryable<TItem> : BaseCollectionQueryable<TItem>
    where TItem : DynamoDBItem, IItem<Primitive>, new()
    {
        private readonly DynamoDBContext _dynamoDBContext;

        public DynamoDBCollectionQueryable(DynamoDBContext dynamoDBContext)
        {
            _dynamoDBContext = dynamoDBContext;
        }

        public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
        {
           await Task.Yield();

            foreach (var item in ApplyPredicates(Predicates, query))
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return item;
            }
        }

        public override Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task<int> DeleteAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private async IEnumerable<TItem> ApplyPredicates(List<QueryItem> predicates, IEnumerable<TItem>)
        {

            var conditions = new List<ScanCondition>();

            var allDocs = await _dynamoDBContext.ScanAsync<TItem>(conditions).GetRemainingAsync();

            IEnumerable<TItem> query = from docs in allDocs select docs;

            foreach (var predicate in predicates)
                query = predicate.QueryType switch
                {
                    QueryType.Where => query.Where(x => predicate.ExpressionBool.Compile().Invoke(x)),
                    QueryType.OrderBy => query.OrderBy(x => predicate.ExpressionObject.Compile().Invoke(x)),
                    QueryType.OrderByDescending => query.OrderByDescending(x => predicate.ExpressionObject.Compile().Invoke(x)),
                    QueryType.ThenBy => (query as IOrderedQueryable<TItem>).ThenBy(predicate.ExpressionObject),
                    QueryType.ThenByDescending => (query as IOrderedQueryable<TItem>)
                        .ThenByDescending(predicate.ExpressionObject),
                    QueryType.Take => predicate.Count.HasValue ? query.Take(predicate.Count.Value) : query,
                    QueryType.Skip => query.Skip(predicate.Count!.Value),
                    _ => query
                };

            return query;
        }

        private async IEnumerable<TItem> GetItemsAsync()
        {
            

            return query;

        }
    }
}
