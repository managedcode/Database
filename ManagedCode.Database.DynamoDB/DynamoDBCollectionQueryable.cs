using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using ManagedCode.Database.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace ManagedCode.Database.DynamoDB;

public class DynamoDBCollectionQueryable<TItem> : BaseCollectionQueryable<TItem> where TItem : class, IItem<Primitive>
{
    private readonly DynamoDBContext _dynamoDBContext;

    public DynamoDBCollectionQueryable(DynamoDBContext dynamoDBContext)
    {
        _dynamoDBContext = dynamoDBContext;
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
    {
        var conditions = new List<ScanCondition>();

        var allDocs = await _dynamoDBContext.ScanAsync<TItem>(conditions).GetRemainingAsync();

        IEnumerable<TItem> items = from docs in allDocs select docs;

        await Task.Yield();

        foreach (var item in ApplyPredicates(Predicates, items))
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return item;
        }
    }

    public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        var conditions = new List<ScanCondition>();

        var allDocs = await _dynamoDBContext.ScanAsync<TItem>(conditions).GetRemainingAsync();

        IEnumerable<TItem> items = from docs in allDocs select docs;

        var query = ApplyPredicates(Predicates, items);

        return await Task.Run(() => query.FirstOrDefault(), cancellationToken);

    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var conditions = new List<ScanCondition>();

        var allDocs = await _dynamoDBContext.ScanAsync<TItem>(conditions).GetRemainingAsync();

        IEnumerable<TItem> items = from docs in allDocs select docs;

        var query = ApplyPredicates(Predicates, items);

        return await Task.Run(() => query.LongCount(), cancellationToken);
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        var conditions = new List<ScanCondition>();

        var allDocs = await _dynamoDBContext.ScanAsync<TItem>(conditions).GetRemainingAsync();

        IEnumerable<TItem> items = from docs in allDocs select docs;

        var ids = ApplyPredicates(Predicates, items)
                        .Select(d => d.Id);


        var result = _dynamoDBContext.DeleteAsync(ids, cancellationToken); //TODO check count

        return Convert.ToInt32(ids.Count());
    }

    private IEnumerable<TItem> ApplyPredicates(List<QueryItem> predicates, IEnumerable<TItem> query)
    {
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
}
