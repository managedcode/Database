using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace ManagedCode.Database.DynamoDB;

public class DynamoDBCollection<TItem> : BaseDatabaseCollection<string, TItem>
    where TItem : DynamoDBItem<string>, IItem<string>, new()
{
    private readonly DynamoDBContext _dynamoDBContext;
    private readonly string _tableName;

    public DynamoDBCollection(DynamoDBContext dynamoDBContext, string tableName)
    { 
        _dynamoDBContext = dynamoDBContext;
        _tableName = tableName;
    }

    public override ICollectionQueryable<TItem> Query => new DynamoDBCollectionQueryable<TItem>(_dynamoDBContext);

    public override void Dispose()
    {
    }

    public override ValueTask DisposeAsync()
    {
        return new ValueTask(Task.CompletedTask);
    }

    #region Get

    protected override async Task<TItem?> GetInternalAsync(string hashKey, CancellationToken cancellationToken = default)
    {
        var cursor = await _dynamoDBContext.QueryAsync<TItem>(hashKey).GetRemainingAsync(cancellationToken);

        return cursor.FirstOrDefault();

    }

    #endregion

    #region Count

    protected override async Task<long> CountInternalAsync(CancellationToken cancellationToken = default)
    {
        var response = await _dynamoDBContext.QueryAsync<TItem>(cancellationToken).GetRemainingAsync();
        return response.Count();
    }

    #endregion

    #region Insert

    protected override async Task<TItem> InsertInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await _dynamoDBContext.SaveAsync<TItem>(item, cancellationToken);

        var response = await _dynamoDBContext.QueryAsync<TItem>(item.Id).GetRemainingAsync(cancellationToken);

        return response.Count == 0 ? null : item;
    }        

    protected override async Task<int> InsertInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        /*List<ScanCondition> conditions = new List<ScanCondition>();

        foreach(var item in items)
        {
            conditions.Add(new ScanCondition("Id", ScanOperator.Equal, item.Id));
        }*/

        var config = new DynamoDBOperationConfig
        {
            OverrideTableName = _tableName,
        };

        foreach(var item in items)
            await _dynamoDBContext.SaveAsync(item, config, cancellationToken);

        var data = await _dynamoDBContext.ScanAsync<TItem>(null).GetRemainingAsync(cancellationToken);

        int responseCount = 0;

        foreach (var item in data)
        {
            if(items.Contains(item))
            {
                responseCount++;
            }
        }

        return responseCount;
    }

    #endregion

    #region InsertOrUpdate

    protected override async Task<TItem> InsertOrUpdateInternalAsync(TItem item,
        CancellationToken cancellationToken = default)
    {
        await _dynamoDBContext.SaveAsync(item, cancellationToken);

        var response = await _dynamoDBContext.QueryAsync<TItem>(item.Id).GetRemainingAsync(cancellationToken);

        return response.Count == 0 ? null : item;
    }

    protected override async Task<int> InsertOrUpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        await _dynamoDBContext.SaveAsync(items, cancellationToken);

        var response = await _dynamoDBContext.QueryAsync<TItem>(items).GetRemainingAsync(cancellationToken);


        return response.Count();
    }

    #endregion

    #region Update

    protected override async Task<TItem> UpdateInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var response = await _dynamoDBContext.QueryAsync<TItem>(item.Id).GetRemainingAsync(cancellationToken);

        if (response.Count == 0)
        {
            throw new DatabaseException("Entity not found in collection.");
        }

        await _dynamoDBContext.SaveAsync(item, cancellationToken);

        var result = await _dynamoDBContext.QueryAsync<TItem>(item.Id).GetRemainingAsync(cancellationToken);

        return result.Count == 0 ? null : item;
    }

    protected override async Task<int> UpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        int count = 0;

        foreach(var item in items)
        {
            var response = await _dynamoDBContext.QueryAsync<TItem>(item.Id).GetRemainingAsync(cancellationToken);

            if (response.Count != 0)
            {
                await _dynamoDBContext.SaveAsync(item, cancellationToken);

                count++;
            }
        }

        return count;
    }

    #endregion

    #region Delete

    protected override async Task<bool> DeleteInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await _dynamoDBContext.DeleteAsync(item, cancellationToken);

        var response = await _dynamoDBContext.QueryAsync<TItem>(item.Id).GetRemainingAsync(cancellationToken);

        return response.Count == 0 ? true : false;
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        int count = 0;

        foreach (var item in items)
        {
            var response = await _dynamoDBContext.QueryAsync<TItem>(item.Id).GetRemainingAsync(cancellationToken);

            if (response.Count != 0)
            {
                await _dynamoDBContext.DeleteAsync(item, cancellationToken);

                count++;
            }
        }

        return count;
    }

    protected override async Task<bool> DeleteCollectionInternalAsync(CancellationToken cancellationToken = default)
    {
        await _dynamoDBContext.DeleteAsync(cancellationToken);

        var response = await _dynamoDBContext.QueryAsync<TItem>(cancellationToken).GetRemainingAsync(cancellationToken);


        return response.Count != 0 ? false : true;
    }

    protected override async Task<bool> DeleteInternalAsync(string id, CancellationToken cancellationToken = default)
    {
        await _dynamoDBContext.DeleteAsync(id, cancellationToken);

        var response = await _dynamoDBContext.QueryAsync<TItem>(id).GetRemainingAsync(cancellationToken);

        return response.Count == 0 ? true : false;
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        int count = 0;

        foreach (var id in ids)
        {
            var response = await _dynamoDBContext.QueryAsync<TItem>(id).GetRemainingAsync(cancellationToken);

            if (response.Count != 0)
            {
                await _dynamoDBContext.DeleteAsync(id, cancellationToken);

                count++;
            }
        }

        return count;
    }

    #endregion

}
