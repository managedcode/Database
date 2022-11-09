using System;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using Microsoft.Azure.Cosmos;

namespace ManagedCode.Database.CosmosDB;

public class CosmosDatabase : BaseDatabase, IDatabase<CosmosClient>
{
    private const int RetryCount = 25;

    private Container? _container;
    private Microsoft.Azure.Cosmos.Database? _database;
    private readonly CosmosDBRepositoryOptions _options;

    public CosmosDatabase(CosmosDBRepositoryOptions options)
    {
        _options = options;

        DBClient = new CosmosClient(options.ConnectionString, options.CosmosClientOptions);
        DBClient.ClientOptions.MaxRetryAttemptsOnRateLimitedRequests = RetryCount;
        DBClient.ClientOptions.MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(2);
    }

    public CosmosClient DBClient { get; }

    protected override async Task InitializeAsyncInternal(CancellationToken token = default)
    {
        if (_options.AllowTableCreation)
        {
            _database = await DBClient.CreateDatabaseIfNotExistsAsync(_options.DatabaseName, cancellationToken: token);

            _container = await _database.CreateContainerIfNotExistsAsync(_options.CollectionName, "/id",
                cancellationToken: token);

            return;
        }

        var database = DBClient.GetDatabase(_options.DatabaseName);

        if (database is null)
        {
            throw new InvalidOperationException($"Database '{_options.DatabaseName}' does not exist.");
        }

        var container = database.GetContainer(_options.CollectionName);

        if (container is null)
        {
            throw new Exception($"Container '{_options.CollectionName}' does not exist.");
        }

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
        DBClient.Dispose();
    }

    public CosmosDBCollection<TItem> GetCollection<TItem>() where TItem : CosmosDBItem, new()
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }

        return new CosmosDBCollection<TItem>(_options, _container!);
    }

    public override async Task Delete(CancellationToken token = default)
    {
        await _database.DeleteAsync(cancellationToken: token);
    }
}