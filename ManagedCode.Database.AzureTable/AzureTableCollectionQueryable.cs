using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables;
using ManagedCode.Database.Core;
using ManagedCode.Database.AzureTable.Extensions;

namespace ManagedCode.Database.AzureTable
{
    public class AzureTableCollectionQueryable<TItem> : BaseCollectionQueryable<TItem>
        where TItem : class, ITableEntity, new()
    {
        private readonly TableClient _tableClient;

        public AzureTableCollectionQueryable(TableClient tableClient)
        {
            _tableClient = tableClient;
        }

        public override IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
        {
            var filter = ConvertPredicatesToFilter(Predicates);
            var query = _tableClient.QueryAsync<TItem>(filter, cancellationToken: cancellationToken);

            return ApplyPredicates(query, Predicates);
        }

        public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
        {
            var filter = ConvertPredicatesToFilter(Predicates);

            return await _tableClient
                .QueryAsync<TItem>(filter, maxPerPage: 1, cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }

        public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
        {
            var filter = ConvertPredicatesToFilter(Predicates);

            return await _tableClient
                .QueryAsync<TItem>(filter, cancellationToken: cancellationToken)
                .CountAsync(cancellationToken);
        }

        public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
        {
            var filter = ConvertPredicatesToFilter(Predicates);

            var items = await _tableClient
                .QueryAsync<TItem>(filter, cancellationToken: cancellationToken).ToListAsync(cancellationToken);

            var responses = await _tableClient.SubmitTransactionByChunksAsync(items,
                TableTransactionActionType.Delete, cancellationToken);

            return responses.Count(v => !v.IsError);
        }

        private static IAsyncEnumerable<TItem> ApplyPredicates(IAsyncEnumerable<TItem> enumerable,
            List<QueryItem> predicates)
        {
            // TODO: add warning
            foreach (var predicate in predicates)
            {
                enumerable = predicate.QueryType switch
                {
                    QueryType.OrderBy => enumerable.OrderBy(x => predicate.ExpressionObject.Compile().Invoke(x)),
                    QueryType.OrderByDescending => enumerable
                        .OrderByDescending(x => predicate.ExpressionObject.Compile().Invoke(x)),
                    QueryType.ThenBy => (enumerable as IOrderedAsyncEnumerable<TItem>)!
                        .ThenBy(x => predicate.ExpressionObject.Compile().Invoke(x)),
                    QueryType.ThenByDescending => (enumerable as IOrderedAsyncEnumerable<TItem>)!
                        .ThenByDescending(x => predicate.ExpressionObject.Compile().Invoke(x)),
                    QueryType.Take => predicate.Count.HasValue ? enumerable.Take(predicate.Count.Value) : enumerable,
                    QueryType.Skip => enumerable.Skip(predicate.Count!.Value),
                    _ => enumerable
                };
            }

            return enumerable;
        }

        private static string ConvertPredicatesToFilter(IEnumerable<QueryItem> predicates)
        {
            var filter = predicates
                .Where(p => p.QueryType is QueryType.Where)
                .Select(p => TableClient.CreateQueryFilter(p.ExpressionBool))
                .Aggregate((a, b) => a + " and " + b);

            return filter;
        }
    }
}