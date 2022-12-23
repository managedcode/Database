using Amazon.Auth.AccessControlPolicy;
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
    private readonly DynamoDBOperationConfig _config;
    private readonly DynamoDBContext _dynamoDBContext;
    private readonly string _tableName;

    public DynamoDBCollection(DynamoDBContext dynamoDBContext, string tableName)
    { 
        _dynamoDBContext = dynamoDBContext;
        _tableName = tableName;
        _config = new DynamoDBOperationConfig
        {
            OverrideTableName = _tableName,
        };
    }

    private List<ScanCondition> GetScanConditions(TItem item)
    {
        return new List<ScanCondition>()
        {
            new ScanCondition("Id", ScanOperator.Equal, item.Id)
        };
    }

    private List<ScanCondition> GetScanConditions(string id)
    {
        return new List<ScanCondition>()
        {
            new ScanCondition("Id", ScanOperator.Equal, id)
        };
    }

    private List<ScanCondition> GetScanConditions(IEnumerable<string> ids)
    {
        var condition = new List<ScanCondition>();

        foreach(var id in ids)
        {
            condition.Add(new ScanCondition("Id", ScanOperator.Equal, id));
        }

        return condition;
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
        var data = await _dynamoDBContext.ScanAsync<TItem>(GetScanConditions(hashKey), _config).GetRemainingAsync(cancellationToken);

        return data.FirstOrDefault();

    }

    #endregion

    #region Count

    protected override async Task<long> CountInternalAsync(CancellationToken cancellationToken = default)
    {
        var data = await _dynamoDBContext.ScanAsync<TItem>(null, _config).GetRemainingAsync(cancellationToken);

        return data.Count;
    }

    #endregion

    #region Insert

    protected override async Task<TItem> InsertInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await _dynamoDBContext.SaveAsync(item, _config, cancellationToken);

        var data = await _dynamoDBContext.ScanAsync<TItem>(GetScanConditions(item), _config).GetRemainingAsync(cancellationToken);

        return data.Count == 0 ? null : item;
    }        

    protected override async Task<int> InsertInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        /*List<ScanCondition> conditions = new List<ScanCondition>();

        foreach(var item in items)
        {
            conditions.Add(new ScanCondition("Id", ScanOperator.Equal, item.Id));
        }*/

        foreach(var item in items)
            await _dynamoDBContext.SaveAsync(item, _config, cancellationToken);

        var data = await _dynamoDBContext.ScanAsync<TItem>(null, _config).GetRemainingAsync(cancellationToken);

        return data.Count;
    }

    #endregion

    #region InsertOrUpdate

    protected override async Task<TItem> InsertOrUpdateInternalAsync(TItem item,
        CancellationToken cancellationToken = default)
    {
        await _dynamoDBContext.SaveAsync(item, _config, cancellationToken);

        var data = await _dynamoDBContext.ScanAsync<TItem>(GetScanConditions(item), _config).GetRemainingAsync(cancellationToken);

        return data.Count == 0 ? null : item;
    }

    protected override async Task<int> InsertOrUpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        foreach (var item in items)
            await _dynamoDBContext.SaveAsync(item, _config, cancellationToken);

        var data = await _dynamoDBContext.ScanAsync<TItem>(null, _config).GetRemainingAsync(cancellationToken);

        return data.Count;
    }

    #endregion

    #region Update

    protected override async Task<TItem> UpdateInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var data = await _dynamoDBContext.ScanAsync<TItem>(GetScanConditions(item), _config).GetRemainingAsync(cancellationToken);

        if (data.Count == 0)
        {
            throw new DatabaseException("Entity not found in collection.");
        }

        await _dynamoDBContext.SaveAsync(item, _config, cancellationToken);

        var result = await _dynamoDBContext.ScanAsync<TItem>(GetScanConditions(item), _config).GetRemainingAsync(cancellationToken);

        return result.Count == 0 ? null : item;
    }

    protected override async Task<int> UpdateInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        int count = 0;

        foreach(var item in items)
        {
            var data = await _dynamoDBContext.ScanAsync<TItem>(GetScanConditions(item), _config).GetRemainingAsync(cancellationToken);

            if (data.Count != 0)
            {
                await _dynamoDBContext.SaveAsync(item, _config, cancellationToken);

                count++;
            }
        }

        return count;
    }

    #endregion

    #region Delete

    protected override async Task<bool> DeleteInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await _dynamoDBContext.DeleteAsync(item, _config, cancellationToken);

        var data = await _dynamoDBContext.ScanAsync<TItem>(GetScanConditions(item), _config).GetRemainingAsync(cancellationToken);

        return data.Count == 0 ? true : false;
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        int count = 0;

        foreach (var item in items)
        {
            var data = await _dynamoDBContext.ScanAsync<TItem>(GetScanConditions(item), _config).GetRemainingAsync(cancellationToken);

            if (data.Count != 0)
            {
                await _dynamoDBContext.DeleteAsync(item, _config, cancellationToken);

                count++;
            }
        }

        return count;
    }

    protected override async Task<bool> DeleteCollectionInternalAsync(CancellationToken cancellationToken = default)
    {
        var data = await _dynamoDBContext.ScanAsync<TItem>(null, _config).GetRemainingAsync(cancellationToken);

        foreach (var item in data)
        {
            await _dynamoDBContext.DeleteAsync(item, _config, cancellationToken);
        }

        data = await _dynamoDBContext.ScanAsync<TItem>(null, _config).GetRemainingAsync(cancellationToken);

        return data.Count != 0 ? false : true;
    }

    protected override async Task<bool> DeleteInternalAsync(string id, CancellationToken cancellationToken = default)
    {
        var data = await _dynamoDBContext.ScanAsync<TItem>(GetScanConditions(id), _config).GetRemainingAsync(cancellationToken);

        await _dynamoDBContext.DeleteAsync(data.FirstOrDefault(), _config, cancellationToken);

        data = await _dynamoDBContext.ScanAsync<TItem>(GetScanConditions(id), _config).GetRemainingAsync(cancellationToken);

        return data.Count == 0 ? true : false;
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        var data = await _dynamoDBContext.ScanAsync<TItem>(GetScanConditions(ids), _config).GetRemainingAsync(cancellationToken);

        await _dynamoDBContext.DeleteAsync(data, _config, cancellationToken);

        var result = await _dynamoDBContext.ScanAsync<TItem>(GetScanConditions(ids), _config).GetRemainingAsync(cancellationToken);

        return data.Count - result.Count;
    }

    #endregion

}
