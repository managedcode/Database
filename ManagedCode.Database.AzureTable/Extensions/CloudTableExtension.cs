using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Database.AzureTable.Extensions;

internal static class CloudTableExtension
{
    private const int RetryCount = 25;
    private const int BatchSize = 100;

    internal static async Task<T?> ExecuteAsync<T>(this CloudTable cloudTable, TableOperation operation,
        CancellationToken token = default)
        where T : class
    {
        var retryCount = RetryCount;
        do
        {
            try
            {
                var result = await cloudTable.ExecuteAsync(operation, token).ConfigureAwait(false);
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

    internal static async Task<object?> ExecuteAsync(this CloudTable cloudTable, TableOperation operation,
        CancellationToken token = default)
    {
        var retryCount = RetryCount;
        do
        {
            try
            {
                var result = await cloudTable.ExecuteAsync(operation, token).ConfigureAwait(false);
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

    internal static async Task<int> ExecuteBatchAsync(this CloudTable cloudTable,
        IEnumerable<TableOperation> operations,
        CancellationToken token = default)
    {
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
                            var result = await cloudTable.ExecuteBatchAsync(batchOperation, token)
                                .ConfigureAwait(false);
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
                        var result = await cloudTable.ExecuteBatchAsync(batchOperation, token).ConfigureAwait(false);
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


    internal static async IAsyncEnumerable<T> ExecuteQuery<T>(this CloudTable cloudTable, TableQuery<T> query,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : ITableEntity, new()
    {
        TableContinuationToken? token = null;

        do
        {
            TableQuerySegment<T>? results = null;
            var retryCount = RetryCount;
            do
            {
                try
                {
                    results = await cloudTable
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

    private static Task WaitRandom(CancellationToken token)
    {
        return Task.Delay(new Random().Next(1000, 3500), token);
    }
}