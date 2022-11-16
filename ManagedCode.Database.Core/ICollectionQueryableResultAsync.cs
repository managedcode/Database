using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ManagedCode.Database.Core
{
    public interface ICollectionQueryableResultAsync<T>
    {
        IAsyncEnumerable<T> ToAsyncEnumerable(CancellationToken cancellationToken = default);
        Task<T?> FirstOrDefaultAsync(CancellationToken cancellationToken = default);
        Task<List<T>> ToListAsync(CancellationToken cancellationToken = default);
        Task<long> CountAsync(CancellationToken cancellationToken = default);
        Task<int> DeleteAsync(CancellationToken cancellationToken = default);
    }
}