using ManagedCode.Database.Core;
using ManagedCode.Database.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ManagedCode.Database.EntityFramework;

public abstract class EFBaseDBCollection<TId, TItem, TContext> :
    BaseDBCollection<TId, TItem>,
    IEFRepository<TId, TItem, TContext>
    where TItem : class, IEFItem<TId>, new()
    where TContext : DbContext
{
}