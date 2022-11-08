using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable;

public class AzureTableAdapter<TItem> where TItem : ITableEntity, new()
{
    private readonly bool _allowTableCreation = true;
    private readonly CloudStorageAccount _cloudStorageAccount;
    private const int RetryCount = 25;
    private readonly object _sync = new();
    private CloudTableClient _cloudTableClient;
    private bool _tableClientInitialized;

    public AzureTableAdapter(string connectionString)
    {
        _cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
    }

    public AzureTableAdapter(StorageCredentials tableStorageCredentials, StorageUri tableStorageUri)
    {
        var cloudStorageAccount = new CloudStorageAccount(tableStorageCredentials, tableStorageUri);
        _cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
    }

    public int BatchSize { get; } = 100;

    private string GetTableName()
    {
        return typeof(TItem).Name.Pluralize();
    }

    public Task<bool> DropTable(CancellationToken token)
    {
        var table = GetTable();
        _tableClientInitialized = false;
        return table.DeleteIfExistsAsync(token);
    }

    private CloudTable GetTable()
    {
        lock (_sync)
        {
            if (_tableClientInitialized)
            {
                return _cloudTableClient.GetTableReference(GetTableName());
            }

            _cloudTableClient = _cloudStorageAccount.CreateCloudTableClient();
            var table = _cloudTableClient.GetTableReference(GetTableName());

            if (_allowTableCreation)
            {
                table.CreateIfNotExists();
            }
            else
            {
                var exists = table.Exists();
                if (!exists)
                {
                    throw new InvalidOperationException($"Table '{GetTableName()}' does not exist");
                }
            }

            _tableClientInitialized = true;
            return table;
        }
    }

    public async Task<T?> ExecuteAsync<T>(TableOperation operation, CancellationToken token = default) where T : class
    {
        var table = GetTable();
        var retryCount = RetryCount;
        do
        {
            try
            {
                var result = await table.ExecuteAsync(operation, token).ConfigureAwait(false);
                return result.Result as T;
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.TooManyRequests)
            {
                retryCount--;
                if (retryCount == 0)
                {
                    throw;
                }

                await WaitRandom(token);
            }
        } while (retryCount > 0);

        throw new Exception(nameof(HttpStatusCode.TooManyRequests));
    }

    public async Task<object?> ExecuteAsync(TableOperation operation, CancellationToken token = default)
    {
        var table = GetTable();
        var retryCount = RetryCount;
        do
        {
            try
            {
                var result = await table.ExecuteAsync(operation, token).ConfigureAwait(false);
                return result.Result;
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.TooManyRequests)
            {
                retryCount--;
                if (retryCount == 0)
                {
                    throw;
                }

                await WaitRandom(token);
            }
        } while (retryCount > 0);

        throw new Exception(nameof(HttpStatusCode.TooManyRequests));
    }

    public async Task<int> ExecuteBatchAsync(IEnumerable<TableOperation> operations, CancellationToken token = default)
    {
        var table = GetTable();
        var totalCount = 0;
        foreach (var group in operations.GroupBy(o => o.Entity.PartitionKey))
        {
            token.ThrowIfCancellationRequested();
            var batchOperation = new TableBatchOperation();
            foreach (var item in group)
            {
                batchOperation.Add(item);
                if (batchOperation.Count == BatchSize)
                {
                    var retryCount = RetryCount;
                    do
                    {
                        try
                        {
                            var result = await table.ExecuteBatchAsync(batchOperation, token).ConfigureAwait(false);
                            totalCount += result.Count;
                            batchOperation = new TableBatchOperation();
                            break;
                        }
                        catch (StorageException e) when (e.RequestInformation.HttpStatusCode ==
                                                         (int)HttpStatusCode.TooManyRequests)
                        {
                            retryCount--;
                            if (retryCount == 0)
                            {
                                throw;
                            }

                            await WaitRandom(token);
                        }
                    } while (retryCount > 0);
                }

                token.ThrowIfCancellationRequested();
            }

            if (batchOperation.Count > 0)
            {
                var retryCount = RetryCount;
                do
                {
                    try
                    {
                        var result = await table.ExecuteBatchAsync(batchOperation, token).ConfigureAwait(false);
                        totalCount += result.Count;
                        break;
                    }
                    catch (StorageException e) when (e.RequestInformation.HttpStatusCode ==
                                                     (int)HttpStatusCode.TooManyRequests)
                    {
                        retryCount--;
                        if (retryCount == 0)
                        {
                            throw;
                        }

                        await WaitRandom(token);
                    }
                } while (retryCount > 0);
            }
        }

        return totalCount;
    }


    public async IAsyncEnumerable<T> ExecuteQuery<T>(TableQuery<T> query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : ITableEntity, new()
    {
        TableContinuationToken? token = null;
        var table = GetTable();

        do
        {
            TableQuerySegment<T>? results = null;
            var retryCount = RetryCount;
            do
            {
                try
                {
                    results = await table
                        .ExecuteQuerySegmentedAsync(query, token, cancellationToken)
                        .ConfigureAwait(false);
                    break;
                }
                catch (StorageException e) when (e.RequestInformation.HttpStatusCode ==
                                                 (int)HttpStatusCode.TooManyRequests)
                {
                    retryCount--;
                    if (retryCount == 0)
                    {
                        throw;
                    }

                    await WaitRandom(cancellationToken);
                }
            } while (retryCount > 0);

            token = results.ContinuationToken;
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var item in results.Results)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return item;
            }
        } while (token is not null);
    }

    private Task WaitRandom(CancellationToken token)
    {
        return Task.Delay(new Random().Next(1000, 3500), token);
    }
}