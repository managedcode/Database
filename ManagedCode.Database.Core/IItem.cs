namespace ManagedCode.Database.Core;

public interface IItem<TId>
{
    TId Id { get; set; }
}