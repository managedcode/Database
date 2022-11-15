using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.MongoDB.Extensions;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace ManagedCode.Database.MongoDB;

public class MongoDBCollectionQueryable<TItem> : BaseDBCollectionQueryable<TItem> where TItem : class, IItem<ObjectId>
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

        foreach (var item in _collection.AsQueryable().ApplyPredicates(Predicates))
        {
            cancellationToken.ThrowIfCancellationRequested();

            yield return item;
        }
    }

    public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        var query = _collection
            .AsQueryable()
            .ApplyPredicates(Predicates.Where(p => p.QueryType is QueryType.Where));

        return await Task.Run(() => query.FirstOrDefault(), cancellationToken);
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        var query = _collection
            .AsQueryable()
            .ApplyPredicates(Predicates.Where(p => p.QueryType is QueryType.Where));

        return await Task.Run(() => query.LongCount(), cancellationToken);
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        var ids = _collection
            .AsQueryable()
            .ApplyPredicates(Predicates.Where(p => p.QueryType is QueryType.Where))
            .Select(d => d.Id);

        var filter = Builders<TItem>.Filter.In(d => d.Id, ids);

        var result = await _collection.DeleteManyAsync(filter, cancellationToken);

        return Convert.ToInt32(result.DeletedCount);
    }
}