using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Tenray.ZoneTree.Collections;
using Tenray.ZoneTree.Comparers;
using Tenray.ZoneTree.Core;
using Tenray.ZoneTree.Options;
using Tenray.ZoneTree.Serializers;
using Tenray.ZoneTree.WAL;

namespace ManagedCode.ZoneTree.Cluster.DB;

public class ZoneTreeOptions<TKey, TValue>
{
    public string Path { get; set; }
    public string ConnectionString { get; set; }
    public StorageType StorageType { get; set; }
    
    public ISerializer<TKey> KeySerializer { get; set; }
    public ISerializer<TValue> ValueSerializer { get; set; }
    public IRefComparer<TKey> Comparer { get; set; }
    
    public  WriteAheadLogMode WALMode { get; set; }
    
    public DiskSegmentMode DiskSegmentMode { get; set; }

}