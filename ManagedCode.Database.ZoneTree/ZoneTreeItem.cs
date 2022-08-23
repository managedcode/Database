using System;

namespace ManagedCode.ZoneTree.Cluster.DB;

public class ZoneTreeItem : IZoneTreeItem<Guid> 
{
    public Guid Id { get; set; } = Guid.NewGuid();
}