using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Humanizer;
using ManagedCode.Repository.Core;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Repository.AzureTable
{
    public class AzureTableAdapter<T> where T : class, ITableEntity, new()
    {
        private readonly bool _allowTableCreation = true;
        private readonly CloudStorageAccount _cloudStorageAccount;
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
            return typeof(T).Name.Pluralize();
        }

        public CloudTable GetTable()
        {
            lock (_sync)
            {
                if (!_tableClientInitialized)
                {
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
                            throw new Exception($"Table '{GetTableName()}' does not exist");
                        }
                    }

                    _tableClientInitialized = true;
                    return table;
                }
            }

            return _cloudTableClient.GetTableReference(GetTableName());
        }

        public async Task<T> ExecuteAsync<T>(TableOperation operation) where T : class
        {
            var table = GetTable();
            var result = await table.ExecuteAsync(operation).ConfigureAwait(false);
            return result.Result as T;
        }

        public async Task<object> ExecuteAsync(TableOperation operation)
        {
            var table = GetTable();
            var result = await table.ExecuteAsync(operation).ConfigureAwait(false);
            return result.Result;
        }

        public async Task<int> ExecuteBatchAsync(IEnumerable<TableOperation> operations)
        {
            var table = GetTable();
            var totalCount = 0;
            foreach (var group in operations.GroupBy(o => o.Entity.PartitionKey))
            {
                var batchOperation = new TableBatchOperation();
                foreach (var item in group)
                {
                    batchOperation.Add(item);
                    if (batchOperation.Count == BatchSize)
                    {
                        try
                        {
                            var result = await table.ExecuteBatchAsync(batchOperation).ConfigureAwait(false);
                            totalCount += result.Count;
                            batchOperation = new TableBatchOperation();
                        }
                        catch (Exception e)
                        {
                            // skip
                        }
                    }
                }

                if (batchOperation.Count > 0)
                {
                    try
                    {
                        var result = await table.ExecuteBatchAsync(batchOperation).ConfigureAwait(false);
                        totalCount += result.Count;
                    }
                    catch (Exception e)
                    {
                        // skip
                    }
                }
            }

            return totalCount;
        }

        public async IAsyncEnumerable<T> Query<T>(
            Expression whereExpression,
            Expression<Func<T, object>> orderByExpression = null,
            Order orderType = Order.By,
            Expression<Func<T, object>> thenByExpression = null,
            Order thenType = Order.By,
            Expression<Func<T, object>> selectExpression = null,
            int? take = null,
            int? skip = null)
            where T : ITableEntity, new()
        {
            string filterString = null;

            if (whereExpression != null)
            {
                filterString = AzureTableQueryTranslator.TranslateExpression(whereExpression);
            }

            List<string> selectedProperties = null;
            if (selectExpression != null)
            {
                selectedProperties = AzureTableQueryPropertyTranslator.TranslateExpressionToMemberNames(selectExpression);
            }

            List<string> orderByProperties = null;
            if (selectExpression != null)
            {
                orderByProperties = AzureTableQueryPropertyTranslator.TranslateExpressionToMemberNames(orderByExpression);
            }

            List<string> thenByProperties = null;
            if (thenByExpression != null)
            {
                thenByProperties = AzureTableQueryPropertyTranslator.TranslateExpressionToMemberNames(thenByExpression);
            }

            TableContinuationToken token = null;
            var table = GetTable();

            var takeCount = 0;

            takeCount = take ?? 1000;

            if (skip.HasValue)
            {
                takeCount += skip.Value;
            }

            var query = new TableQuery<T>
            {
                FilterString = filterString,
                TakeCount = takeCount > 1000 ? 1000 : takeCount
            };

            if (selectedProperties != null)
            {
                query.Select(selectedProperties);
            }

            if (orderByProperties != null)
            {
                foreach (var item in orderByProperties)
                {
                    if (orderType == Order.By)
                    {
                        query.OrderBy(item);
                    }
                    else
                    {
                        query.OrderByDesc(item);
                    }
                }
            }

            if (thenByProperties != null)
            {
                foreach (var item in thenByProperties)
                {
                    if (thenType == Order.By)
                    {
                        query.OrderBy(item);
                    }
                    else
                    {
                        query.OrderByDesc(item);
                    }
                }
            }

            long count = 0;
            long skipCount = 0;

            do
            {
                var results = await table.ExecuteQuerySegmentedAsync(query, token)
                    .ConfigureAwait(false);

                token = results.ContinuationToken;

                foreach (var item in results.Results)
                {
                    if (skip > 0 && skipCount < skip)
                    {
                        skipCount++;
                        continue;
                    }

                    count++;
                    yield return item;

                    if (take > 0 && count >= take)
                    {
                        break;
                    }
                }

                if (take > 0 && count >= take)
                {
                    break;
                }
            } while (token != null);
        }
    }
}