using System;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using Microsoft.Azure.Cosmos;

namespace ManagedCode.Database.Cosmos;

public class CosmosDatabase : BaseDatabase<CosmosClient>
{
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
        
        if (_options.MaxRetryAttemptsOnRateLimitedRequests.HasValue)
        {
            cosmosClient.ClientOptions.MaxRetryAttemptsOnRateLimitedRequests = _options.MaxRetryAttemptsOnRateLimitedRequests.Value;
        }
        if (_options.MaxRetryWaitTimeOnRateLimitedRequests.HasValue)
        {
            cosmosClient.ClientOptions.MaxRetryWaitTimeOnRateLimitedRequests = _options.MaxRetryWaitTimeOnRateLimitedRequests.Value;
        }
        

        if (_options.AllowTableCreation)
        {
            _database = await cosmosClient.CreateDatabaseIfNotExistsAsync(_options.DatabaseName, cancellationToken: token);
            _container = await _database.CreateContainerIfNotExistsAsync(_options.CollectionName, _options.PartitionKey, cancellationToken: token);
        }

        var database = cosmosClient.GetDatabase(_options.DatabaseName);

        if (database is null)
            throw new InvalidOperationException($"Database '{_options.DatabaseName}' does not exist.");

        var container = database.GetContainer(_options.CollectionName);

        if (container is null) 
            throw new Exception($"Container '{_options.CollectionName}' does not exist.");
        
        _database = database;
        _container = container;
        NativeClient = cosmosClient;
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        DisposeInternal();
        return ValueTask.CompletedTask;
    }

    protected override void DisposeInternal()
    {
        NativeClient?.Dispose();
        _database = null;
        _container = null;
        IsInitialized = false;
    }

    public CosmosCollection<TItem> GetCollection<TItem>() where TItem : CosmosItem, new()
    {
        if (!IsInitialized) 
            throw new DatabaseNotInitializedException(GetType());

        return new CosmosCollection<TItem>(_options, _container!);
    }

    public override async Task DeleteAsync(CancellationToken token = default)
    {
        if (_database is not null)
        {
            await _database.DeleteAsync(cancellationToken: token);
        }
    }
}