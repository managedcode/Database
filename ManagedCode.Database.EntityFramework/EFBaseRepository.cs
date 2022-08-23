using ManagedCode.Database.Core;
using ManagedCode.Database.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ManagedCode.Database.EntityFramework;

public abstract class EFBaseRepository<TId, TItem, TContext> :
    BaseRepository<TId, TItem>,
    IEFRepository<TId, TItem, TContext>
    where TItem : class, IEFItem<TId>, new()
    where TContext : DbContext
{
}