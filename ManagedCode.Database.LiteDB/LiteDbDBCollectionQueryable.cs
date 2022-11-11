using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using ManagedCode.Database.Core;
using ManagedCode.Database.LiteDB.Extensions;

namespace ManagedCode.Database.LiteDB;

public class LiteDbDBCollectionQueryable<TId, TItem> : BaseDBCollectionQueryable<TItem>
    where TItem : LiteDbItem<TId>, IItem<TId>, new()
{
    private readonly ILiteCollection<TItem> _collection;

    public LiteDbDBCollectionQueryable(ILiteCollection<TItem> collection)
    {
        _collection = collection;
    }

    private IEnumerable<TItem> GetItemsInternal()
    {
        var items = _collection.Query().ToEnumerable();

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
                    items = items.Take(query.Count.GetValueOrDefault());
                    break;

                case QueryType.Skip:
                    items = items.Skip(query.Count.GetValueOrDefault());
                    break;

                default:
                    break;
            }
        }

        return items;
    }

    public override async IAsyncEnumerable<TItem> ToAsyncEnumerable(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        foreach (var item in GetItemsInternal())
        {
            yield return item;
        }
    }

    public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        return GetItemsInternal().FirstOrDefault();
    }

    public override async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        int count = 0;

        foreach (var item in GetItemsInternal())
        {
            count++;
        }

        return count;
    }

    public override async Task<int> DeleteAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        int count = 0;

        foreach (var item in GetItemsInternal())
        {
            _collection.DeleteMany(item => true);
            count++;
        }

        return count;
    }
}