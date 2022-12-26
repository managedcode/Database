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
    private AmazonDynamoDBClient _dynamoDBClient;
    private readonly string _tableName;

    public DynamoDBCollection(DynamoDBContext dynamoDBContext, AmazonDynamoDBClient dynamoDBClient, string tableName)
    { 
        _dynamoDBContext = dynamoDBContext;
        _dynamoDBClient = dynamoDBClient;
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
        var batchWriter = _dynamoDBContext.CreateBatchWrite<TItem>(_config);

         batchWriter.AddPutItems(items);

        await batchWriter.ExecuteAsync();

        return items.Count(); //TODO check count of added items
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
        var batchWriter = _dynamoDBContext.CreateBatchWrite<TItem>(_config);

        batchWriter.AddPutItems(items);

        await batchWriter.ExecuteAsync();

        return items.Count(); //TODO check count of added items
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

        foreach (var item in items)
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
        var batchWriter = _dynamoDBContext.CreateBatchWrite<TItem>(_config);

        batchWriter.AddDeleteItems(items);

        await batchWriter.ExecuteAsync();

        return items.Count(); //TODO check count of added items
    }

    protected override async Task<bool> DeleteCollectionInternalAsync(CancellationToken cancellationToken = default)
    {
        DeleteTableRequest deleteTableRequest = new DeleteTableRequest
        {
            TableName = _tableName,
        };
        
        await _dynamoDBClient.DeleteTableAsync(deleteTableRequest);

        bool result = false;

        try
        {
            Table.LoadTable(_dynamoDBClient, _tableName); //TODO Edit check table
        }
        catch
        {
            result = true;
        }

        return result;
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
        var data = new List<TItem>();

        foreach(var id in ids)
        {
            var item = await _dynamoDBContext.ScanAsync<TItem>(GetScanConditions(id), _config).GetRemainingAsync(cancellationToken);
            data.Add(item.First());
        }

        var batchWriter = _dynamoDBContext.CreateBatchWrite<TItem>(_config);

        batchWriter.AddDeleteItems(data);

        await batchWriter.ExecuteAsync();

        return ids.Count(); //TODO check count of added items
    }

    #endregion

}
