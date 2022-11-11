using System;
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

    private IEnumerable<KeyValuePair<TId, TItem>> GetItemsInternal()
    {
        IEnumerable<KeyValuePair<TId, TItem>> items = _collection.FindAll().ToAsyncEnumerable();

        foreach (var query in Predicates)
        {
            switch (query.QueryType)
            {
                case QueryType.Where:
                    items = items.Where(x => query.ExpressionBool.Compile().Invoke(x.Value));
                    break;

                case QueryType.OrderBy:
                    /*if (items is IOrderedEnumerable<KeyValuePair<TId, TItem>>)
                    {
                        throw new InvalidOperationException("After OrderBy call ThenBy.");
                    }*/
                    items = items.OrderBy(x => query.ExpressionObject.Compile().Invoke(x.Value));
                    break;

                case QueryType.OrderByDescending:
                    /*if (items is IOrderedEnumerable<KeyValuePair<TId, TItem>>)
                    {
                        throw new InvalidOperationException("After OrderBy call ThenBy.");

                    }*/
                    items = items.OrderByDescending(x => query.ExpressionObject.Compile().Invoke(x.Value));
                    break;

                case QueryType.ThenBy: //TODO Add Exception
                /*if (items is IOrderedEnumerable<KeyValuePair<TId, TItem>> orderedItems)
                {
                    items = orderedItems.ThenBy(x => query.ExpressionObject.Compile().Invoke(x));
                    break;
                }
                throw new InvalidOperationException("Before ThenBy call first OrderBy.");*/

                case QueryType.ThenByDescending:
                    /*if (items is IOrderedEnumerable<KeyValuePair<TId, TItem>> orderedDescendingItems)
                    {
                        items = orderedDescendingItems.ThenByDescending(x => query.ExpressionObject.Compile().Invoke(x));
                        break;
                    }
                    throw new InvalidOperationException("Before ThenBy call first OrderBy.");*/

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
            cancellationToken.ThrowIfCancellationRequested();

            yield return item.Value;
        }
    }

   public override async Task<TItem?> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
    {
       /* await Task.Yield();

        return _collection.Query()
            .Where(WherePredicates)
            .FirstOrDefault();*/
       throw new NotImplementedException();
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
        /*await Task.Yield();

        // TODO: check

        return _collection.DeleteMany(WherePredicates.First());*/
        throw new NotImplementedException();

    }
}