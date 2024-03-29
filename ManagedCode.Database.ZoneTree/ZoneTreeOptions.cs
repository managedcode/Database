using Tenray.ZoneTree.Options;

namespace ManagedCode.Database.ZoneTree;

public class ZoneTreeOptions
{
    public string Path { get; set; }
    public string ConnectionString { get; set; }
    public StorageType StorageType { get; set; }
    public WriteAheadLogMode WALMode { get; set; }
    public DiskSegmentMode DiskSegmentMode { get; set; }
}