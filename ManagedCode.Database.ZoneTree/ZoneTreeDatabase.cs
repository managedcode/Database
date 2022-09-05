using System;
using ManagedCode.Database.Core;
using ManagedCode.Database.Core.Common;
using ManagedCode.Database.Core.InMemory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tenray.ZoneTree.Serializers;

namespace ManagedCode.ZoneTree.Cluster.DB;

public class ZoneTreeDatabase : BaseDatabase, IDatabase<ZoneTreeDatabase>
{
    private readonly ILogger<ZoneTreeDatabase> _logger;
    private readonly string _path;
    private Dictionary<string, IDisposable> _collection = new ();
    public ZoneTreeDatabase(ILogger<ZoneTreeDatabase> logger, string path)
    {
        _logger = logger;
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

    public ZoneTreeDatabase DataBase { get; }
    
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
        
        lock (DataBase)
        {
            var className = typeof(TItem).FullName;
            if(_collection.TryGetValue(className, out var collection))
            {
                return (ZoneTreeDBCollection<TId, TItem> )collection;
            }
            else
            {
                var newCollection = new ZoneTreeDBCollection<TId, TItem> (_logger, Path.Combine(_path, className));
                _collection.Add(className, newCollection);
                return newCollection;
            }
        }
    }
    

}