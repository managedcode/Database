using System;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using Microsoft.Azure.Cosmos;

namespace ManagedCode.Database.Cosmos;

public class CosmosDatabase : BaseDatabase<CosmosClient>
{
    private const int RetryCount = 25;
    private readonly CosmosOptions _options;

    private Container? _container;
    private Microsoft.Azure.Cosmos.Database? _database;

    public CosmosDatabase(CosmosOptions options)
    {
        _options = options;
    }

    protected override async Task InitializeAsyncInternal(CancellationToken token = default)
    {
        var cosmosClient = new CosmosClient(_options.ConnectionString, _options.CosmosClientOptions);
        cosmosClient.ClientOptions.MaxRetryAttemptsOnRateLimitedRequests = RetryCount;
        cosmosClient.ClientOptions.MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(2);
        NativeClient = cosmosClient;
        
        if (_options.AllowTableCreation)
        {
            _database = await cosmosClient.CreateDatabaseIfNotExistsAsync(_options.DatabaseName, cancellationToken: token);
            _container = await _database.CreateContainerIfNotExistsAsync(_options.CollectionName, _options.PartitionKey, cancellationToken: token);
        }
        else
        {
            _database = cosmosClient.GetDatabase(_options.DatabaseName);
            
            if (_database is null)
                throw new InvalidOperationException($"Database '{_options.DatabaseName}' does not exist.");

            _container = _database.GetContainer(_options.CollectionName);
             
            if (_container is null) 
                throw new Exception($"Container '{_options.CollectionName}' does not exist.");
        }
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        DisposeInternal();
        return ValueTask.CompletedTask;
    }

    protected override void DisposeInternal()
    {
        _database?.Client.Dispose();
        NativeClient?.Dispose();
    }

    public CosmosCollection<TItem> GetCollection<TItem>() where TItem : CosmosItem, new()
    {
        if (!IsInitialized) 
            throw new DatabaseNotInitializedException(GetType());

        return new CosmosCollection<TItem>(_options, _container!);
    }

    public override async Task DeleteAsync(CancellationToken token = default)
    {
        if (_database is null)
            return;
        
        await _database.DeleteAsync(cancellationToken: token);
    }
}