using System;
using System.Collections.Generic;
using System.Linq;
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
        var items = _collection.AsQueryable();

        foreach (var query in Predicates)
        {
            switch (query.QueryType)
            {
                case QueryType.Where:
                    items = items.Where(x => query.ExpressionBool.Compile().Invoke(x));
                    break;

                case QueryType.OrderBy:
                    if (items is IOrderedEnumerable<TItem>)
                    {
                        throw new InvalidOperationException("LiteBD does not support multiple OrderBy.");
                    }

                    items = items.OrderBy(x => query.ExpressionObject.Compile().Invoke(x));
                    break;

                case QueryType.OrderByDescending:
                    if (items is IOrderedEnumerable<TItem>)
                    {
                        throw new InvalidOperationException("LiteBD does not support multiple OrderBy.");
                    }

                    items = items.OrderByDescending(x => query.ExpressionObject.Compile().Invoke(x));
                    break;

                case QueryType.ThenBy:
                    throw new InvalidOperationException("LiteBD does not support ThenBy.");

                case QueryType.ThenByDescending:
                    throw new InvalidOperationException("LiteBD does not support ThenBy.");

                case QueryType.Take:
                    if (query.Count.HasValue)
                    {
                        items = items.Take(query.Count.Value);
                    }
                    break;

                case QueryType.Skip:
                    if (query.Count.HasValue)
                    {
                        items = items.Skip(query.Count.Value);
                    }
                    break;

                default:
                    break;
            }
        }

        return items;
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        foreach (var item in GetItemsInternal())
        {
            yield return item;
        }
    }

    public override Task<TItem> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        int count = 0;

        foreach (var item in GetItemsInternal())
        {
            count++;
        }

        return await Task.Run(() => count, cancellationToken);
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        var ids = GetItemsInternal().Select(d => d.Id);

        var filter = Builders<TItem>.Filter.In(d => d.Id, ids);

        var result = await _collection.DeleteManyAsync(filter);

        return Convert.ToInt32(result.DeletedCount);
    }
}