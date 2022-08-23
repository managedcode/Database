using System;
using System.Collections.Generic;
using Tenray.ZoneTree;
using Tenray.ZoneTree.AbstractFileStream;
using Tenray.ZoneTree.BlobFileSystem;
using Tenray.ZoneTree.Maintainers;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ManagedCode.ZoneTree.Cluster.DB;

public class ZoneTreeWrapper<TKey, TValue> : IDisposable
{
    private readonly ILogger _logger;
    private readonly string _path;
    private IZoneTree<TKey, TValue?> _zoneTree;
    private BasicZoneTreeMaintainer<TKey, TValue?> _maintainer;

    public ZoneTreeWrapper(ILogger logger, string path)
    {
        _logger = logger;
        _path = path;
    }
    public void Open(ZoneTreeOptions<TKey, TValue?> options)
    {
        /*IFileStreamProvider streamProvider = options.StorageType switch
        {
            StorageType.Blob => new BlobFileStreamProvider(options.ConnectionString, options.Path),
            StorageType.File => new LocalFileStreamProvider(),
            _ => throw new ArgumentOutOfRangeException(nameof(options.StorageType), options.StorageType, null)
        };*/

        var factory = new ZoneTreeFactory<TKey, TValue?>(new LocalFileStreamProvider())
            .SetLogger(new WrapperLogger(_logger))
            
            //.SetKeySerializer(options.KeySerializer)
            .SetValueSerializer(options.ValueSerializer)
            //.SetComparer(options.Comparer) //StringOrdinalComparerAscending()
            
            .SetDataDirectory(_path)
            .SetWriteAheadLogDirectory(_path)
            
            .SetIsValueDeletedDelegate((in TValue? value) => value == null)
            .SetMarkValueDeletedDelegate((ref TValue? value) => value = default)
            
            //.SetIsValueDeletedDelegate((in Deletable<TValue> x) => x.IsDeleted)
            //.SetMarkValueDeletedDelegate((ref Deletable<TValue> x) => x.IsDeleted = true)


            .ConfigureWriteAheadLogProvider(x =>
            {
                // x.CompressionBlockSize = 1024 * 1024 * 20;
                x.WriteAheadLogMode = options.WALMode;
                //x.EnableIncrementalBackup = true;
            })
            .Configure(x =>
            {
                x.EnableDiskSegmentCompression = true;
                x.DiskSegmentMode = options.DiskSegmentMode;
                x.DiskSegmentCompressionBlockSize = 1024 * 1024 * 30;
                //x.DiskSegmentMinimumRecordCount = 10_000;
                //x.DiskSegmentMaximumRecordCount = 100_000;
                x.DiskSegmentBlockCacheLimit = 2;
                x.MutableSegmentMaxItemCount = 10_000; // number of data im memory  
            });
        
        _zoneTree = factory.OpenOrCreate();
        _maintainer = new (_zoneTree);
    }

    
    public void Maintenance()
    {
        _maintainer.CompleteRunningTasks();
        _zoneTree.Maintenance.MoveSegmentZeroForward();
        _zoneTree.Maintenance.StartMergeOperation()?.Join();
    }
    public void Dispose()
    {
        _maintainer?.CompleteRunningTasks();
        _maintainer?.Dispose();
        _zoneTree?.Dispose();
    }
    
    public bool Insert(TKey key, TValue value)
    {
        return _zoneTree.TryAtomicAdd(key, value);
    }
    
    public void Update(TKey key, TValue value)
    {
        _zoneTree.TryAtomicUpdate(key, value);
    }
    
    public void InsertOrUpdate(TKey key, TValue value)
    {
        _zoneTree.TryAtomicAddOrUpdate(key, value, (ref TValue? old) => value);
    }
    
    public void Upsert(TKey key, TValue value)
    {
        _zoneTree.Upsert(key, value);
    }
    
    public TValue Get(TKey key)
    {
        if(_zoneTree.TryGet(in key, out var value))
        {
            return value;
        }
        
        return default;
    }
    
    public bool Contains(TKey key)
    {
        return _zoneTree.ContainsKey(key);
    }

    public void Delete(TKey key)
    {
        _zoneTree.ForceDelete(in key);
    }

    public int Count()
    {
        //add more cache logic
        return _zoneTree.Count();
    }

    public IEnumerable<TValue?> Enumerate()
    {
        using var iterator = _zoneTree.CreateIterator();
        while (iterator.Next())
        {
            yield return iterator.CurrentValue;
        }
    }
    
    public IEnumerable<TValue?> EnumerateReverse()
    {
        using var iterator = _zoneTree.CreateReverseIterator();
        while (iterator.Next())
        {
            yield return iterator.CurrentValue;
        }
    }

}