using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.Core;

public interface IDBCollection<in TId, TItem> : IDisposable, IAsyncDisposable where TItem : IItem<TId>
{
    IDBCollectionQueryable<TItem> Query { get; }

    Task<TItem> InsertAsync(TItem item, CancellationToken cancellationToken = default);
    Task<int> InsertAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default);

    Task<TItem> UpdateAsync(TItem item, CancellationToken cancellationToken = default);
    Task<int> UpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default);

    Task<TItem> InsertOrUpdateAsync(TItem item, CancellationToken cancellationToken = default);
    Task<int> InsertOrUpdateAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(TItem item, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(IEnumerable<TItem> items, CancellationToken cancellationToken = default);

    Task<bool> DeleteCollectionAsync(CancellationToken cancellationToken = default);

    Task<TItem?> GetAsync(TId id, CancellationToken cancellationToken = default);

    Task<long> CountAsync(CancellationToken cancellationToken = default);
}