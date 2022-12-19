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

namespace ManagedCode.Database.DynamoDB;

public class DynamoDBDatabase : BaseDatabase<AmazonDynamoDBClient>
{
    private readonly Dictionary<string, AttributeValue> _collections = new();

    private readonly DynamoDBOptions _dbOptions;
    private DynamoDBContext _dynamoDBContext;

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
        var client = new AmazonDynamoDBClient(creds, config);
        _dynamoDBContext = new DynamoDBContext(client);

        return Task.CompletedTask;
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
    }

    public DynamoDBCollection<TItem> GetCollection<TItem>()
       where TItem : DynamoDBItem, new()
    {
        if (!IsInitialized) throw new DatabaseNotInitializedException(GetType());

        return new DynamoDBCollection<TItem>(_dynamoDBContext);
    }
}
