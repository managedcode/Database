using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;

namespace ManagedCode.ZoneTree.Cluster.DB;

public class ZoneTreeDatabase : BaseDatabase, IDatabase<ZoneTreeDatabase>
{
    private readonly string _path;
    private Dictionary<string, IDisposable> _collection = new();

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
        foreach (var table in _collection)
        {
            table.Value.Dispose();
        }
    }

    public override Task Delete(CancellationToken token = default)
    {
        throw new NotImplementedException();
    }

    public ZoneTreeDatabase DBClient { get; }

    public ZoneTreeDBCollection<TId, TItem> GetCollection<TId, TItem>() where TItem : IItem<TId>
    {
        return GetCollection<TId, TItem>(typeof(TItem).FullName);
    }

    public ZoneTreeDBCollection<TId, TItem> GetCollection<TId, TItem>(string name) where TItem : IItem<TId>
    {
        if (!IsInitialized)
        {
            throw new DatabaseNotInitializedException(GetType());
        }

        lock (DBClient)
        {
            var className = typeof(TItem).FullName;
            if (_collection.TryGetValue(className, out var collection))
            {
                return (ZoneTreeDBCollection<TId, TItem>)collection;
            }
            else
            {
                var newCollection = new ZoneTreeDBCollection<TId, TItem>(Path.Combine(_path, className));
                _collection.Add(className, newCollection);
                return newCollection;
            }
        }
    }
}