namespace ManagedCode.Database.ZoneTree
{
    public interface IZoneTreeItem<T> 
    {
        public new T Id { get; set; }
    }
}