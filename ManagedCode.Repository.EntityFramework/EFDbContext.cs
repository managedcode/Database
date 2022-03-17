using System;
using System.Collections.Generic;
using ManagedCode.Repository.Core;
using ManagedCode.Repository.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ManagedCode.Repository.EntityFramework;

public class EFDbContext<TContext> : DbContext where TContext : EFDbContext<TContext>
{
    private readonly IList<Type> _entityTypes;

    public EFDbContext(DbContextOptions<TContext> options, ServiceCollectionHolder serviceCollectionHolder) : base(options)
    {
        _entityTypes = new List<Type>();

        foreach (var item in serviceCollectionHolder.ServiceCollection)
        {
            var belongs = item.ImplementationType?.BaseType?.BaseType.EqualsToGeneric(typeof(EFRepository<,,>));

            if (belongs != null && belongs.Value)
            {
                var typeParameters = item.ImplementationType.BaseType.GetGenericArguments();

                if (typeParameters.Length == 2)
                {
                    _entityTypes.Add(typeParameters[1]);
                }
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entityMethod = typeof(ModelBuilder).GetMethod("Entity", new[] { typeof(Type) });

        foreach (var type in _entityTypes)
        {
            entityMethod
                .Invoke(modelBuilder, new object[] { type });
        }
    }
}