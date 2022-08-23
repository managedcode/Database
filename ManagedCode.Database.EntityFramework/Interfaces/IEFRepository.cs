using ManagedCode.Database.Core;
using Microsoft.EntityFrameworkCore;

namespace ManagedCode.Database.EntityFramework.Interfaces;

public interface IEFRepository<TId, TItem, TContext> : IRepository<TId, TItem>
    where TItem : IEFItem<TId>, new()
    where TContext : DbContext
{
}