using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.Core;

public interface IDBCollection<in TId, TItem> : IDisposable, IAsyncDisposable where TItem : IItem<TId>
{
    Task<TItem> InsertAsync(TItem item, CancellationToken token = default);
    Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken token = default);

    Task<TItem> UpdateAsync(TItem item, CancellationToken token = default);
    Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken token = default);

    Task<TItem> InsertOrUpdateAsync(TItem item, CancellationToken token = default);
    Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items, CancellationToken token = default);

    Task<bool> DeleteAsync(TId id, CancellationToken token = default);
    Task<bool> DeleteAsync(TItem item, CancellationToken token = default);
    Task<int> DeleteAsync(IEnumerable<TId> ids, CancellationToken token = default);
    Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken token = default);

    Task<bool> DeleteAllAsync(CancellationToken token = default);

    Task<TItem> GetAsync(TId id, CancellationToken token = default);
    
    Task<long> CountAsync(CancellationToken token = default);
    
    IDBCollectionQueryable<TItem> Query { get; }
}