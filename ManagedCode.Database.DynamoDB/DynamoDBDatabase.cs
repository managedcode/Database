using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;

namespace ManagedCode.Database.DynamoDB;

public class DynamoDBDatabase : BaseDatabase<AmazonDynamoDBClient>
{
    private readonly Dictionary<string, AttributeValue> _collections = new();

    private readonly DynamoDBOptions _dbOptions;
    private DynamoDBContext _dynamoDBContext;
    private AmazonDynamoDBClient _dynamoDBClient;
    private DynamoDBOperationConfig _dynamoDBOperationConfig;

    public DynamoDBDatabase(DynamoDBOptions dbOptions)
    {
        _dbOptions = dbOptions;
    }



    public override Task DeleteAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        var creds = new BasicAWSCredentials(_dbOptions.AccessKey, _dbOptions.SecretKey);
        var config = new AmazonDynamoDBConfig()
        {
            ServiceURL = _dbOptions.ServiceURL,
            AuthenticationRegion = _dbOptions.AuthenticationRegion,
        };
        _dynamoDBClient = new AmazonDynamoDBClient(creds, config);
        _dynamoDBContext = new DynamoDBContext(_dynamoDBClient);

        return Task.CompletedTask;
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
    }

    public async Task<CreateTableResponse> SetupAsync(string tableName)
    {
        var createTableRequest = new CreateTableRequest
        {
            TableName = tableName,
            AttributeDefinitions = new List<AttributeDefinition>(),
            KeySchema = new List<KeySchemaElement>(),
            GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>(),
            LocalSecondaryIndexes = new List<LocalSecondaryIndex>(),
            ProvisionedThroughput = new ProvisionedThroughput
            {
                ReadCapacityUnits = 1,
                WriteCapacityUnits = 1
            }
        };
        createTableRequest.KeySchema = new[] {
            new KeySchemaElement {
            AttributeName = "Id",
                KeyType = KeyType.HASH,
            },

        }.ToList();

        createTableRequest.AttributeDefinitions = new[] {
            new AttributeDefinition {
            AttributeName = "Id",
                AttributeType = ScalarAttributeType.N,
            },
        }.ToList();

        var createTableResponse = _dynamoDBClient.CreateTableAsync(createTableRequest);

        return createTableResponse.Result;
    }

    public  DynamoDBCollection<TItem> GetCollection<TItem>()
       where TItem : DynamoDBItem<string>, new()
    {
        if (!IsInitialized) throw new DatabaseNotInitializedException(GetType());

        var tableName = string.IsNullOrEmpty(_dbOptions.CollectionName)
            ? typeof(TItem).Name.Pluralize()
            : _dbOptions.CollectionName;

        SetupAsync(tableName).GetAwaiter().GetResult();

        Table table = Table.LoadTable(_dynamoDBClient, tableName);

        return new DynamoDBCollection<TItem>(_dynamoDBContext, tableName);
    }
}
