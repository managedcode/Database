﻿using ManagedCode.Repository.Core;
using ManagedCode.Repository.EntityFramework.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ManagedCode.Repository.EntityFramework;

public abstract class EFBaseRepository<TId, TItem, TContext> :
    BaseRepository<TId, TItem>,
    IEFRepository<TId, TItem, TContext>
    where TItem : class, IEFItem<TId>, new()
    where TContext : DbContext
{
}