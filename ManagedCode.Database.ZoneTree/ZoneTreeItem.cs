namespace ManagedCode.Database.ZoneTree
{
    public class ZoneTreeItem : IZoneTreeItem<Guid> 
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}