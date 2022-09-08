using System;
using ManagedCode.Database.EntityFramework.MSSQL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Database.EntityFramework.MSSQL.Extensions;

public static class ProviderBuilderExtensions
{
    public static IServiceCollection AddMSSQLBasedOnEF(this IServiceCollection serviceCollection, Action<MSSQLConnectionOptions> action)
    {
        var connectionOptions = new MSSQLConnectionOptions();
        action.Invoke(connectionOptions);

        serviceCollection.AddDbContext<MSSQLDatabaseContext>(options => options
            .UseSqlServer(connectionOptions.ConnectionString)
            .UseQueryTrackingBehavior(
                connectionOptions.UseTracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking
            )
        );

        return serviceCollection;
    }
}