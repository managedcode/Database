using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace ManagedCode.Database.MongoDB;

public class MongoDbDBCollectionQueryable<TItem> : BaseDBCollectionQueryable<TItem> where TItem : class, IItem<ObjectId>
{
    private readonly IMongoCollection<TItem> _collection;

    public MongoDbDBCollectionQueryable(IMongoCollection<TItem> collection)
    {
        _collection = collection;
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
    {
        var query = _collection.AsQueryable();
        foreach (var predicate in WherePredicates)
        {
            query = query.Where(predicate);
        }

        if (OrderByPredicates.Count > 0)
        {
            foreach (var predicate in OrderByPredicates)
            {
                query = query.OrderBy(predicate);
            }
        }

        if (OrderByDescendingPredicates.Count > 0)
        {
            foreach (var predicate in OrderByDescendingPredicates)
            {
                query = query.OrderByDescending(predicate);
            }
        }

        SkipValue ??= 0;

        if (TakeValue.HasValue)
        {
            foreach (var item in await Task.Run(() => query.Skip(SkipValue.Value).Take(TakeValue.Value).ToArray(), cancellationToken))
            {
                yield return item;
            }
        }
        else
        {
            foreach (var item in await Task.Run(() => query.Skip(SkipValue.Value).ToArray(), cancellationToken))
            {
                yield return item;
            }
        }
    }

    public override Task<TItem> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var query = _collection.AsQueryable();
        foreach (var predicate in WherePredicates)
        {
            query = query.Where(predicate);
        }

        return await Task.Run(() => query.Count(), cancellationToken);
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        var result = await _collection.DeleteManyAsync<TItem>(WherePredicates.FirstOrDefault(), cancellationToken);
        return Convert.ToInt32(result.DeletedCount);
    }
}