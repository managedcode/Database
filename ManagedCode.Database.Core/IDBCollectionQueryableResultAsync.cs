using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.Core.Queries;

public interface IDBCollectionQueryableResultAsync<T>
{
    IAsyncEnumerable<T> ToAsyncEnumerable(CancellationToken cancellationToken = default);
    Task<long> LongCountAsync(CancellationToken cancellationToken = default);
    Task<int> DeleteAsync(CancellationToken cancellationToken = default);
}