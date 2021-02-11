using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ManagedCode.Repository.Core;
using Microsoft.Azure.Cosmos.Table;

namespace ManagedCode.Repository.AzureTable
{
    public class AzureTableRepository<TId, TItem> : BaseRepository<TId, TItem>
        where TId : AzureTableId
        where TItem : class, IRepositoryItem<TId>, ITableEntity, new()
    {
        private readonly AzureTableAdapter<TItem> _tableAdapter;

        public AzureTableRepository(string connectionString)
        {
            _tableAdapter = new AzureTableAdapter<TItem>(connectionString);
        }

        public AzureTableRepository(StorageCredentials tableStorageCredentials, StorageUri tableStorageUri)
        {
            _tableAdapter = new AzureTableAdapter<TItem>(tableStorageCredentials, tableStorageUri);
        }

        protected override Task InitializeAsyncInternal(CancellationToken token = default)
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        #region Insert

        protected override async Task<bool> InsertAsyncInternal(TItem item, CancellationToken token = default)
        {
            try
            {
                var result = await _tableAdapter.ExecuteAsync(TableOperation.Insert(item), token);
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<int> InsertAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            try
            {
                return await _tableAdapter.ExecuteBatchAsync(items.Select(s => TableOperation.Insert(s)), token);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        #endregion

        #region InsertOrUpdate

        protected override async Task<bool> InsertOrUpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            try
            {
                var result = await _tableAdapter.ExecuteAsync(TableOperation.InsertOrReplace(item), token);
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            try
            {
                return await _tableAdapter.ExecuteBatchAsync(items.Select(s => TableOperation.InsertOrReplace(s)), token);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        #endregion

        #region Update

        protected override async Task<bool> UpdateAsyncInternal(TItem item, CancellationToken token = default)
        {
            try
            {
                if (string.IsNullOrEmpty(item.ETag))
                {
                    item.ETag = "*";
                }

                var result = await _tableAdapter.ExecuteAsync(TableOperation.Replace(item), token);
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<int> UpdateAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            try
            {
                return await _tableAdapter.ExecuteBatchAsync(items.Select(s =>
                {
                    if (string.IsNullOrEmpty(s.ETag))
                    {
                        s.ETag = "*";
                    }

                    return TableOperation.Replace(s);
                }), token);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        #endregion

        #region Delete

        protected override async Task<bool> DeleteAsyncInternal(TId id, CancellationToken token = default)
        {
            try
            {
                var result = await _tableAdapter.ExecuteAsync(TableOperation.Delete(new DynamicTableEntity(id.PartitionKey, id.RowKey)
                {
                    ETag = "*"
                }), token);
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<bool> DeleteAsyncInternal(TItem item, CancellationToken token = default)
        {
            try
            {
                var result = await _tableAdapter.ExecuteAsync(TableOperation.Delete(new DynamicTableEntity(item.PartitionKey, item.RowKey)
                {
                    ETag = "*"
                }), token);
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TId> ids, CancellationToken token = default)
        {
            try
            {
                return await _tableAdapter.ExecuteBatchAsync(ids.Select(s => TableOperation.Delete(new DynamicTableEntity(s.PartitionKey, s.RowKey)
                {
                    ETag = "*"
                })), token);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> items, CancellationToken token = default)
        {
            try
            {
                return await _tableAdapter.ExecuteBatchAsync(items.Select(s => TableOperation.Delete(new DynamicTableEntity(s.PartitionKey, s.RowKey)
                {
                    ETag = "*"
                })), token);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        protected override async Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            var count = 0;
            var totalCount = 0;

            do
            {
                token.ThrowIfCancellationRequested();
                var ids = await _tableAdapter
                    .Query<DynamicTableEntity>(predicate, selectExpression: item => new DynamicTableEntity(item.PartitionKey, item.RowKey),
                        take: _tableAdapter.BatchSize)
                    .ToListAsync();

                count = ids.Count;

                token.ThrowIfCancellationRequested();
                totalCount += await _tableAdapter.ExecuteBatchAsync(ids.Select(s => TableOperation.Delete(new DynamicTableEntity(s.PartitionKey, s.RowKey)
                {
                    ETag = "*"
                })), token);
            } while (count > 0);

            return totalCount;
        }

        protected override Task<bool> DeleteAllAsyncInternal(CancellationToken token = default)
        {
            return _tableAdapter.DropTable(token);
        }

        #endregion

        #region Get

        protected override Task<TItem> GetAsyncInternal(TId id, CancellationToken token = default)
        {
            return _tableAdapter.ExecuteAsync<TItem>(TableOperation.Retrieve<TItem>(id.PartitionKey, id.RowKey), token);
        }

        protected override async Task<TItem> GetAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            var item = await _tableAdapter.Query<TItem>(predicate, take: 1, cancellationToken: token).ToListAsync(token);
            return item.FirstOrDefault();
        }

        #endregion

        #region Find

        protected override IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            return _tableAdapter.Query<TItem>(predicate, take: take, skip: skip, cancellationToken: token);
        }

        protected override IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            return _tableAdapter.Query(predicate, orderBy, orderType, take: take, skip: skip, cancellationToken: token);
        }

        protected override IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            Expression<Func<TItem, object>> thenBy,
            Order thenType,
            int? take = null,
            int skip = 0,
            CancellationToken token = default)
        {
            return _tableAdapter.Query(predicate, orderBy, orderType, thenBy, thenType, take: take, skip: skip);
        }

        #endregion

        #region Count

        protected override async Task<uint> CountAsyncInternal(CancellationToken token = default)
        {
            uint count = 0;

            Expression<Func<TItem, bool>> predicate = item => true;

            await foreach (var item in _tableAdapter
                .Query<DynamicTableEntity>(predicate, selectExpression: item => new DynamicTableEntity(item.PartitionKey, item.RowKey),
                    cancellationToken: token))
            {
                count++;
            }

            return count;
        }

        protected override async Task<uint> CountAsyncInternal(Expression<Func<TItem, bool>> predicate, CancellationToken token = default)
        {
            uint count = 0;

            await foreach (var item in _tableAdapter
                .Query<DynamicTableEntity>(predicate, selectExpression: item => new DynamicTableEntity(item.PartitionKey, item.RowKey),
                    cancellationToken: token))
            {
                count++;
            }

            return count;
        }

        #endregion
    }
}