using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace ManagedCode.Database.CosmosDB;

public class CosmosDbAdapter<T> where T : class, new()
{
    private readonly bool _allowTableCreation = true;
    private readonly string _collectionName;
    private readonly CosmosClient _cosmosClient;
    private readonly string _databaseName;
    private const int RetryCount = 25;
    private bool _tableClientInitialized;

    public CosmosDbAdapter(string connectionString, CosmosClientOptions cosmosClientOptions, string databaseName,
        string collectionName)
    {
        _databaseName = string.IsNullOrEmpty(databaseName) ? "database" : databaseName;
        _collectionName = string.IsNullOrEmpty(collectionName) ? "container" : collectionName;

        _cosmosClient = new CosmosClient(connectionString, cosmosClientOptions);
        _cosmosClient.ClientOptions.MaxRetryAttemptsOnRateLimitedRequests = RetryCount;
        _cosmosClient.ClientOptions.MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(2);
    }

    public async Task<Container> GetContainer()
    {
        if (!_tableClientInitialized)
        {
            if (_allowTableCreation)
            {
                var response = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseName);
                var database = response.Database;
                await database.CreateContainerIfNotExistsAsync(_collectionName, "/id")
                    .ConfigureAwait(false);
            }
            else
            {
                var database = _cosmosClient.GetDatabase(_databaseName);
                if (database == null)
                {
                    throw new Exception($"Database '{_databaseName}' does not exist.");
                }

                var container = database.GetContainer(_collectionName);
                if (container == null)
                {
                    throw new Exception($"Container '{_collectionName}' does not exist.");
                }
            }

            _tableClientInitialized = true;
        }

        return _cosmosClient.GetContainer(_databaseName, _collectionName);
    }
}