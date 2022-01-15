using ManagedCode.Repository.Core;
using Microsoft.EntityFrameworkCore;

namespace ManagedCode.Repository.EntityFramework
{
    public interface IEFRepository<TId, TItem, TContext> : IRepository<TId, TItem> 
        where TItem : IEFItem<TId>, new()
        where TContext : DbContext
    {
    }
}
