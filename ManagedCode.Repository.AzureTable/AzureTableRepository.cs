using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        protected override Task InitializeAsyncInternal()
        {
            IsInitialized = true;
            return Task.CompletedTask;
        }

        #region Insert

        protected override async Task<bool> InsertAsyncInternal(TItem item)
        {
            try
            {
                var result = await _tableAdapter.ExecuteAsync(TableOperation.Insert(item));
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<int> InsertAsyncInternal(IEnumerable<TItem> items)
        {
            try
            {
                return await _tableAdapter.ExecuteBatchAsync(items.Select(s => TableOperation.Insert(s)));
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        #endregion

        #region InsertOrUpdate

        protected override async Task<bool> InsertOrUpdateAsyncInternal(TItem item)
        {
            try
            {
                var result = await _tableAdapter.ExecuteAsync(TableOperation.InsertOrReplace(item));
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<int> InsertOrUpdateAsyncInternal(IEnumerable<TItem> items)
        {
            try
            {
                return await _tableAdapter.ExecuteBatchAsync(items.Select(s => TableOperation.InsertOrReplace(s)));
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        #endregion

        #region Update

        protected override async Task<bool> UpdateAsyncInternal(TItem item)
        {
            try
            {
                if (string.IsNullOrEmpty(item.ETag))
                {
                    item.ETag = "*";
                }

                var result = await _tableAdapter.ExecuteAsync(TableOperation.Replace(item));
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<int> UpdateAsyncInternal(IEnumerable<TItem> items)
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
                }));
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        #endregion

        #region Delete

        protected override async Task<bool> DeleteAsyncInternal(TId id)
        {
            try
            {
                var result = await _tableAdapter.ExecuteAsync(TableOperation.Delete(new DynamicTableEntity(id.PartitionKey, id.RowKey)
                {
                    ETag = "*"
                }));
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<bool> DeleteAsyncInternal(TItem item)
        {
            try
            {
                var result = await _tableAdapter.ExecuteAsync(TableOperation.Delete(new DynamicTableEntity(item.PartitionKey, item.RowKey)
                {
                    ETag = "*"
                }));
                return result != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TId> ids)
        {
            try
            {
                return await _tableAdapter.ExecuteBatchAsync(ids.Select(s => TableOperation.Delete(new DynamicTableEntity(s.PartitionKey, s.RowKey)
                {
                    ETag = "*"
                })));
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        protected override async Task<int> DeleteAsyncInternal(IEnumerable<TItem> items)
        {
            try
            {
                return await _tableAdapter.ExecuteBatchAsync(items.Select(s => TableOperation.Delete(new DynamicTableEntity(s.PartitionKey, s.RowKey)
                {
                    ETag = "*"
                })));
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        protected override async Task<int> DeleteAsyncInternal(Expression<Func<TItem, bool>> predicate)
        {
            var count = 0;
            var totalCount = 0;

            do
            {
                var ids = await _tableAdapter
                    .Query<DynamicTableEntity>(predicate, selectExpression: item => new DynamicTableEntity(item.PartitionKey, item.RowKey),
                        take: _tableAdapter.BatchSize)
                    .ToListAsync();

                count = ids.Count;

                totalCount += await _tableAdapter.ExecuteBatchAsync(ids.Select(s => TableOperation.Delete(new DynamicTableEntity(s.PartitionKey, s.RowKey)
                {
                    ETag = "*"
                })));
            } while (count > 0);

            return totalCount;
        }

        protected override async Task<bool> DeleteAllAsyncInternal()
        {
            await DeleteAsyncInternal(item => true);
            return true;
        }

        #endregion

        #region Get

        protected override Task<TItem> GetAsyncInternal(TId id)
        {
            return _tableAdapter.ExecuteAsync<TItem>(TableOperation.Retrieve<TItem>(id.PartitionKey, id.RowKey));
        }

        protected override async Task<TItem> GetAsyncInternal(Expression<Func<TItem, bool>> predicate)
        {
            var item = await _tableAdapter.Query<TItem>(predicate, take: 1).ToListAsync();
            return item.FirstOrDefault();
        }

        #endregion

        #region Find

        protected override IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate, int? take = null, int skip = 0)
        {
            return _tableAdapter.Query<TItem>(predicate, take: take, skip: skip);
        }

        protected override IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            int? take = null,
            int skip = 0)
        {
            return _tableAdapter.Query(predicate, orderBy, orderType, take: take, skip: skip);
        }

        protected override IAsyncEnumerable<TItem> FindAsyncInternal(Expression<Func<TItem, bool>> predicate,
            Expression<Func<TItem, object>> orderBy,
            Order orderType,
            Expression<Func<TItem, object>> thenBy,
            Order thenType,
            int? take = null,
            int skip = 0)
        {
            return _tableAdapter.Query(predicate, orderBy, orderType, thenBy, thenType, take: take, skip: skip);
        }

        #endregion

        #region Count

        protected override async Task<uint> CountAsyncInternal()
        {
            uint count = 0;

            Expression<Func<TItem, bool>> predicate = item => true;

            await foreach (var item in _tableAdapter
                .Query<DynamicTableEntity>(predicate, selectExpression: item => new DynamicTableEntity(item.PartitionKey, item.RowKey)))
            {
                count++;
            }

            return count;
        }

        protected override async Task<uint> CountAsyncInternal(Expression<Func<TItem, bool>> predicate)
        {
            uint count = 0;

            await foreach (var item in _tableAdapter
                .Query<DynamicTableEntity>(predicate, selectExpression: item => new DynamicTableEntity(item.PartitionKey, item.RowKey)))
            {
                count++;
            }

            return count;
        }

        #endregion
    }
}