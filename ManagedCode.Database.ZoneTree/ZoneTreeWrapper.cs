using Tenray.ZoneTree;
using Tenray.ZoneTree.AbstractFileStream;
using Tenray.ZoneTree.Options;

namespace ManagedCode.ZoneTree.Cluster.DB;

public class ZoneTreeWrapper<TKey, TValue> : IDisposable
{
    private readonly string _path;
    private IZoneTree<TKey, TValue?> _zoneTree;
    private IMaintainer _maintainer;

    public ZoneTreeWrapper(string path)
    {
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
            //.SetKeySerializer(options.KeySerializer)
            .SetValueSerializer(options.ValueSerializer)
            //.SetComparer(options.Comparer) //StringOrdinalComparerAscending()
            .SetDataDirectory(_path)
            .SetWriteAheadLogDirectory(_path)
            .SetIsValueDeletedDelegate((in TValue? value) => value == null)
            .SetMarkValueDeletedDelegate((ref TValue? value) => value = default)

            //.SetIsValueDeletedDelegate((in Deletable<TValue> x) => x.IsDeleted)
            //.SetMarkValueDeletedDelegate((ref Deletable<TValue> x) => x.IsDeleted = true)
            .ConfigureWriteAheadLogOptions(x =>
            {
                // x.CompressionBlockSize = 1024 * 1024 * 20;
                x.WriteAheadLogMode = options.WALMode;
                //x.EnableIncrementalBackup = true;
            })
            .Configure(x =>
            {
                x.DiskSegmentOptions.CompressionMethod = CompressionMethod.Brotli;
                x.DiskSegmentOptions.CompressionBlockSize = 1024 * 1024 * 30;
                x.DiskSegmentOptions.DiskSegmentMode = options.DiskSegmentMode;
                x.DiskSegmentOptions.BlockCacheLimit = 2;
                x.MutableSegmentMaxItemCount = 10_000; // number of data im memory  
            });


        _zoneTree = factory.OpenOrCreate();
        _maintainer = _zoneTree.CreateMaintainer();
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
        _zoneTree.TryAtomicAddOrUpdate(key, value, (ref TValue? val) => true);
    }

    public void Upsert(TKey key, TValue value)
    {
        _zoneTree.Upsert(key, value);
    }

    public TValue Get(TKey key)
    {
        if (_zoneTree.TryGet(in key, out var value))
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

    public void DeleteAll()
    {
        _zoneTree.Maintenance.DestroyTree();
    }

    public long Count()
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