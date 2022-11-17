using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;

namespace ManagedCode.Database.ZoneTree;

public class ZoneTreeDatabase : BaseDatabase<ZoneTreeDatabase>
{
    private readonly string _path;
    private readonly Dictionary<string, IDisposable> _collection = new();

    public ZoneTreeDatabase(string path)
    {
        _path = path;
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
        foreach (var table in _collection) table.Value.Dispose();
    }

    public override Task DeleteAsync(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public ZoneTreeDatabaseCollection<TId, TItem> GetCollection<TId, TItem>() where TItem : IItem<TId>
    {
        return GetCollection<TId, TItem>(typeof(TItem).FullName);
    }

    public ZoneTreeDatabaseCollection<TId, TItem> GetCollection<TId, TItem>(string name) where TItem : IItem<TId>
    {
        if (!IsInitialized) throw new DatabaseNotInitializedException(GetType());

        lock (NativeClient)
        {
            var className = typeof(TItem).FullName;
            if (_collection.TryGetValue(className, out var collection))
            {
                return (ZoneTreeDatabaseCollection<TId, TItem>)collection;
            }

            var newCollection = new ZoneTreeDatabaseCollection<TId, TItem>(Path.Combine(_path, className));
            _collection.Add(className, newCollection);
            return newCollection;
        }
    }
}