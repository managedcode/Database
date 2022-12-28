using Amazon.Auth.AccessControlPolicy;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.Model.Internal.MarshallTransformations;
using Amazon.Runtime.Internal;
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
using System.Transactions;
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

    private DeleteItemRequest DeleteItemRequestById(string id)
    {
        return new DeleteItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>()
                    {
                        {"Id", new AttributeValue() {S = id}}
                    },
            ReturnValues = "ALL_OLD",
        };
    }

    public override ICollectionQueryable<TItem> Query => new DynamoDBCollectionQueryable<TItem>(_dynamoDBContext, _tableName);

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
        var response = await _dynamoDBClient.DeleteItemAsync(DeleteItemRequestById(item.Id), cancellationToken);

        if (response.Attributes.Count != 0 && response.HttpStatusCode == System.Net.HttpStatusCode.OK)
        {
            return true;
        }

        return false;
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<TItem> items,
        CancellationToken cancellationToken = default)
    {
        var count = 0;

        IEnumerable<TItem[]> itemsChunk = items.Chunk(100);

        foreach (var itemsList in itemsChunk)
        {
            var tasks = new List<Task>();

            foreach (var item in itemsList)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var response = await _dynamoDBClient.DeleteItemAsync(DeleteItemRequestById(item.Id), cancellationToken);

                    if (response.Attributes.Count != 0 && response.HttpStatusCode == System.Net.HttpStatusCode.OK) 
                        Interlocked.Increment(ref count);
                }));
            }

            await Task.WhenAll(tasks);
        }

        return count;
    }

protected override async Task<bool> DeleteCollectionInternalAsync(CancellationToken cancellationToken = default)
    {
        DeleteTableRequest deleteTableRequest = new DeleteTableRequest
        {
            TableName = _tableName,
        };

        var response = await _dynamoDBClient.DeleteTableAsync(deleteTableRequest);

        if(response.HttpStatusCode == System.Net.HttpStatusCode.OK)
        {
            return true;
        }

        return false;
    }

    protected override async Task<bool> DeleteInternalAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _dynamoDBClient.DeleteItemAsync(DeleteItemRequestById(id), cancellationToken);

        if (response.Attributes.Count != 0 && response.HttpStatusCode == System.Net.HttpStatusCode.OK)
        {
            return true;
        }

        return false;
    }

    protected override async Task<int> DeleteInternalAsync(IEnumerable<string> ids, CancellationToken cancellationToken = default)
    {
        var count = 0;

        IEnumerable<string[]> idsChunk = ids.Chunk(100);

        foreach (var idsList in idsChunk)
        {
            var tasks = new List<Task>();

            foreach (var id in idsList)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var response = await _dynamoDBClient.DeleteItemAsync(DeleteItemRequestById(id), cancellationToken);

                    if (response.Attributes.Count != 0 && response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        Interlocked.Increment(ref count);
                }));
            }

            await Task.WhenAll(tasks);
        }

        return count;
    }

    #endregion

}
