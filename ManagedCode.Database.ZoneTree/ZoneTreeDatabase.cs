using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;

namespace ManagedCode.Database.ZoneTree;

public class ZoneTreeDatabase : BaseDatabase<ZoneTreeDatabase>
{
    private readonly ZoneTreeOptions _options;
    private readonly Dictionary<string, IDisposable> _collection = new();

    public ZoneTreeDatabase(ZoneTreeOptions options)
    {
        _options = options;
    }

    protected override Task InitializeAsyncInternal(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    protected override async ValueTask DisposeAsyncInternal()
    {
        await Task.Run(Dispose);
    }

    protected override void DisposeInternal()
    {
        lock (NativeClient)
        {
            foreach (var table in _collection)
            {
                table.Value.Dispose();
            }
        }
    }

    public override async Task DeleteAsync(CancellationToken token = default)
    {
        await Task.Yield();
        Directory.Delete(_options.Path, true);
    }

    public ZoneTreeCollection<TId, TItem> GetCollection<TId, TItem>() where TItem : IItem<TId>
    {
        if (!IsInitialized) throw new DatabaseNotInitializedException(GetType());

        lock (NativeClient)
        {
            var collectionName = typeof(TItem).FullName!;

            if (_collection.TryGetValue(collectionName, out var collection))
            {
                return (ZoneTreeCollection<TId, TItem>)collection;
            }

            ZoneTreeCollectionOptions<TId, TItem> collectionOptions = new()
            {
                Path = Path.Combine(_options.Path, collectionName),
                WALMode = _options.WALMode,
                DiskSegmentMode = _options.DiskSegmentMode,
                StorageType = _options.StorageType,
                ValueSerializer = new JsonSerializer<TItem>()
            };

            var newCollection = new ZoneTreeCollection<TId, TItem>(collectionOptions);
            _collection.Add(collectionName, newCollection);
            return newCollection;
        }
    }
}