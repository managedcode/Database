using System;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using Microsoft.Azure.Cosmos;

namespace ManagedCode.Database.CosmosDB;

public class CosmosDatabase : BaseDatabase<CosmosClient>
{
    private const int RetryCount = 25;

    private Container? _container;
    private Microsoft.Azure.Cosmos.Database? _database;
    private readonly CosmosDBRepositoryOptions _options;

    public CosmosDatabase(CosmosDBRepositoryOptions options)
    {
        _options = options;
    }

    protected override async Task InitializeAsyncInternal(CancellationToken token = default)
    {
        var cosmosClient = new CosmosClient(_options.ConnectionString, _options.CosmosClientOptions);
        cosmosClient.ClientOptions.MaxRetryAttemptsOnRateLimitedRequests = RetryCount;
        cosmosClient.ClientOptions.MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(2);

        if (_options.AllowTableCreation)
        {
            _database = await cosmosClient.CreateDatabaseIfNotExistsAsync(_options.DatabaseName,
                cancellationToken: token);

            _container = await _database.CreateContainerIfNotExistsAsync(_options.CollectionName, "/id",
                cancellationToken: token);

            return;
        }

        var database = cosmosClient.GetDatabase(_options.DatabaseName);

        if (database is null)
        {
            throw new InvalidOperationException($"Database '{_options.DatabaseName}' does not exist.");
        }

        var container = database.GetContainer(_options.CollectionName);

        if (container is null)
        {
            throw new Exception($"Container '{_options.CollectionName}' does not exist.");
        }

        NativeClient = cosmosClient;
        _database = database;
        _container = container;
    }

    protected override ValueTask DisposeAsyncInternal()
    {
        DisposeInternal();
        return new ValueTask(Task.CompletedTask);
    }

    protected override void DisposeInternal()
    {
        NativeClient.Dispose();
    }

    public CosmosDBCollection<TItem> GetCollection<TItem>() where TItem : CosmosDBItem, new()
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }

        return new CosmosDBCollection<TItem>(_options, _container!);
    }

    public override async Task DeleteAsync(CancellationToken token = default)
    {
        await _database.DeleteAsync(cancellationToken: token);
    }
}