namespace ManagedCode.Repository.Core
{
    public interface IRepositoryItem<TId>
    {
        TId Id { get; set; }
    }
}