﻿using Amazon.DynamoDBv2.DataModel;
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

public class DynamoDBCollectionQueryable<TItem> : BaseCollectionQueryable<TItem> where TItem : class, IItem<string>
{
    private readonly DynamoDBContext _dynamoDBContext;
    private readonly DynamoDBOperationConfig _config;

    public DynamoDBCollectionQueryable(DynamoDBContext dynamoDBContext, string tableName)
    {
        _dynamoDBContext = dynamoDBContext;
        _config = new DynamoDBOperationConfig
        {
            OverrideTableName = tableName,
        };
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
    {
        var data = await _dynamoDBContext.ScanAsync<TItem>(null, _config).GetRemainingAsync();

        await Task.Yield();

        foreach (var item in ApplyPredicates(Predicates, data.AsQueryable()))
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return item;
        }
    }

    public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        var data = await _dynamoDBContext.ScanAsync<TItem>(null, _config).GetRemainingAsync();

        var query = ApplyPredicates(Predicates, data.AsQueryable());

        return await Task.Run(() => query.FirstOrDefault(), cancellationToken);

    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var data = await _dynamoDBContext.ScanAsync<TItem>(null, _config).GetRemainingAsync();

        var query = ApplyPredicates(Predicates, data.AsQueryable());

        return await Task.Run(() => query.LongCount(), cancellationToken);
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        var data = await _dynamoDBContext.ScanAsync<TItem>(null, _config).GetRemainingAsync();

        var items = ApplyPredicates(Predicates, data.AsQueryable());

        var count = 0;

        IEnumerable<TItem[]> itemsChunk = items.Chunk(25);

        foreach (var itemsList in itemsChunk)
        {
            var tasks = new List<Task>();

            foreach (var item in itemsList)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var batchWriter = _dynamoDBContext.CreateBatchWrite<TItem>(_config);

                    batchWriter.AddDeleteItem(item);

                    await batchWriter.ExecuteAsync();

                    Interlocked.Increment(ref count);
                }));
            }

            await Task.WhenAll(tasks);
        }

        return count;
    }

    private IQueryable<TItem> ApplyPredicates(List<QueryItem> predicates, IQueryable<TItem> query)
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
