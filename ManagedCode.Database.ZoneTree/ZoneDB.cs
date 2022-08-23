using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Tenray.ZoneTree.Serializers;

namespace ManagedCode.ZoneTree.Cluster.DB;

public class ZoneDB : IDisposable, IAsyncDisposable
{
    private readonly ILogger<ZoneDB> _logger;
    private readonly string _path;
    private Dictionary<string, IDisposable> _collection = new ();
    
    public ZoneDB( string path) : this(NullLogger<ZoneDB>.Instance, path)
    {

    }
    
    public ZoneDB(ILogger<ZoneDB> logger, string path)
    {
        _logger = logger;
        _path = path;
    }
    
    public DbCollection<TValue> GetCollection<TValue>() where TValue : ZoneTreeItem 
    {
        var className = typeof(TValue).FullName;
        if(_collection.TryGetValue(className, out var collection))
        {
            return (DbCollection<TValue>)collection;
        }
        else
        {
            var newCollection = new DbCollection<TValue>(_logger, Path.Combine(_path, className));
            _collection.Add(className, newCollection);
            return newCollection;
        }
            
    }

    public void Dispose()
    {
        foreach (var table in _collection)
        {
            table.Value.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Task.Run(Dispose);
    }
}