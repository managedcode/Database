using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DnsClient;
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

    private IEnumerable<TItem> GetItemsInternal()
    {
        var mongoQuery = _collection.AsQueryable();

        foreach (var query in Predicates)
        {
            switch (query.QueryType)
            {
                case QueryType.Where:
                    mongoQuery = mongoQuery.Where(x => query.ExpressionBool.Compile().Invoke(x));
                    break;

                case QueryType.OrderBy:
                    if (mongoQuery is IOrderedMongoQueryable<TItem>)
                    {
                        throw new InvalidOperationException("After OrderBy call ThenBy.");
                    }
                    mongoQuery = mongoQuery.OrderBy(x => query.ExpressionObject.Compile().Invoke(x));
                    break;

                case QueryType.OrderByDescending:
                    if (mongoQuery is IOrderedMongoQueryable<TItem>)
                    {
                        throw new InvalidOperationException("After OrderBy call ThenBy.");
                    }
                    mongoQuery = mongoQuery.OrderByDescending(x => query.ExpressionObject.Compile().Invoke(x));
                    break;

                case QueryType.ThenBy:
                    if (mongoQuery is IOrderedMongoQueryable<TItem> orderedItems)
                    {
                        mongoQuery = orderedItems.ThenBy(x => query.ExpressionObject.Compile().Invoke(x));
                        break;
                    }
                    throw new InvalidOperationException("Before ThenBy call first OrderBy.");

                case QueryType.ThenByDescending:
                    if (mongoQuery is IOrderedMongoQueryable<TItem> orderedDescendingItems)
                    {
                        mongoQuery = orderedDescendingItems.ThenBy(x => query.ExpressionObject.Compile().Invoke(x));
                        break;
                    }
                    throw new InvalidOperationException("Before ThenBy call first OrderBy.");

                case QueryType.Take:
                    if (query.Count.HasValue)
                    {
                        mongoQuery = mongoQuery.Take(query.Count.Value);
                    }
                    break;

                case QueryType.Skip:
                    if (query.Count.HasValue)
                    {
                        mongoQuery = mongoQuery.Skip(query.Count.Value);
                    }
                    break;

                default:
                    break;
            }
        }

        return mongoQuery;
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        foreach (var item in GetItemsInternal())
        {
            yield return item;
        }
    }

    public override Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        return await Task.Run(() => GetItemsInternal().Count(), cancellationToken);
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        var ids = GetItemsInternal().Select(d => d.Id);

        var filter = Builders<TItem>.Filter.In(d => d.Id, ids);

        var result = await _collection.DeleteManyAsync(filter);

        return Convert.ToInt32(result.DeletedCount);
    }
}