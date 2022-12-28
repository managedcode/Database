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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Transactions;

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

    private List<ScanCondition> GetScanConditions(string id)
    {
        return new List<ScanCondition>()
        {
            new ScanCondition("Id", ScanOperator.Equal, id)
        };
    }

    private GetItemRequest GetItemRequestById(string id)
    {
        return new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue>()
            {
                {"Id", new AttributeValue() {S = id}}
            },
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

    private async Task<Document> PutItemRequestByTItemAsync(TItem item, CancellationToken cancellationToken)
    {
        PutItemOperationConfig config = new PutItemOperationConfig
        {
            ReturnValues = ReturnValues.UpdatedNewAttributes,
        };

        var table = Table.LoadTable(_dynamoDBClient, _tableName);

        return await table.PutItemAsync(Document.FromJson(JsonSerializer.Serialize(item)), config, cancellationToken);
    }

    private async Task<Document> UpdateItemRequestByTItemAsync(TItem item, CancellationToken cancellationToken)
    {
        UpdateItemOperationConfig config = new UpdateItemOperationConfig
        {
            ReturnValues = ReturnValues.AllOldAttributes,
        };

        var table = Table.LoadTable(_dynamoDBClient, _tableName);

        return await table.UpdateItemAsync(Document.FromJson(JsonSerializer.Serialize(item)), config, cancellationToken);
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
        await PutItemRequestByTItemAsync(item, cancellationToken);

        return item;
    }        

    protected override async Task<int> InsertInternalAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
    {
        var count = 0;

        IEnumerable<TItem[]> itemsChunk = items.Chunk(25);

        foreach (var itemsList in itemsChunk)
        {
            var tasks = new List<Task>();

            foreach (var item in itemsList)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var response = await PutItemRequestByTItemAsync(item, cancellationToken);

                    /*var batchWriter = _dynamoDBContext.CreateBatchWrite<TItem>(_config);

                    batchWriter.AddPutItem(item);

                    await batchWriter.ExecuteAsync();
                    */

                    Interlocked.Increment(ref count);
                }));
            }

            await Task.WhenAll(tasks);
        }

        return count;
    }

    #endregion

    #region InsertOrUpdate

    protected override async Task<TItem> InsertOrUpdateInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        await PutItemRequestByTItemAsync(item, cancellationToken);

        return item;
    }

    protected override async Task<int> InsertOrUpdateInternalAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
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
                    var response = await PutItemRequestByTItemAsync(item, cancellationToken);

                    Interlocked.Increment(ref count);
                }));
            }

            await Task.WhenAll(tasks);
        }

        return count;
    }

    #endregion

    #region Update

    protected override async Task<TItem> UpdateInternalAsync(TItem item, CancellationToken cancellationToken = default)
    {
        var response = await UpdateItemRequestByTItemAsync(item, cancellationToken);

        return response.Count() != 0 ? item : null;
    }

    protected override async Task<int> UpdateInternalAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
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
                    var response = await UpdateItemRequestByTItemAsync(item, cancellationToken);

                    if (response.Count() != 0)
                        Interlocked.Increment(ref count);
                }));
            }

            await Task.WhenAll(tasks);
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

    protected override async Task<int> DeleteInternalAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default)
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
