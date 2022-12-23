using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace ManagedCode.Database.Cosmos;

public class CosmosCollectionQueryable<TItem> : BaseCollectionQueryable<TItem>
    where TItem : CosmosItem, IItem<string>, new()
{
    private readonly Container _container;
    private readonly IQueryable<TItem> _query;

    public CosmosCollectionQueryable(Container container, IQueryable<TItem> query)
    {
        _container = container;
        _query = query;
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var feedIterator = ApplyPredicates(Predicates).ToFeedIterator();

        using (var iterator = feedIterator)
        {
            while (iterator.HasMoreResults)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var item in await iterator.ReadNextAsync(cancellationToken)) yield return item;
            }
        }
    }

    public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        var queryIterator = ApplyPredicates(Predicates).ToFeedIterator();
        var response = await queryIterator.ReadNextAsync();
        return response.FirstOrDefault();
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var query = ApplyPredicates(Predicates);
        return await query.CountAsync(cancellationToken);
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        var count = 0;

        var asyncEnumerable = ToAsyncEnumerable(cancellationToken);

        await Parallel.ForEachAsync(asyncEnumerable, cancellationToken, async (item, token) =>
        {
            await _container.DeleteItemAsync<TItem>(item.Id, item.PartitionKey, cancellationToken: token);
            Interlocked.Increment(ref count);
        });

        return count;
    }

    private IQueryable<TItem> ApplyPredicates(IEnumerable<QueryItem> predicates)
    {
        var query = _query;

        foreach (var predicate in predicates)
            query = predicate.QueryType switch
            {
                QueryType.Where => query.Where(predicate.ExpressionBool),
                QueryType.OrderBy => query.OrderBy(predicate.ExpressionObject),
                QueryType.OrderByDescending => query.OrderByDescending(predicate.ExpressionObject),
                QueryType.ThenBy => (query as IOrderedQueryable<TItem>)!.ThenBy(predicate.ExpressionObject),
                QueryType.ThenByDescending => (query as IOrderedQueryable<TItem>)!
                    .ThenByDescending(predicate.ExpressionObject),
                QueryType.Take => predicate.Count.HasValue ? query.Take(predicate.Count.Value) : query,
                QueryType.Skip => query.Skip(predicate.Count!.Value),
                _ => query
            };

        return query;
    }
}