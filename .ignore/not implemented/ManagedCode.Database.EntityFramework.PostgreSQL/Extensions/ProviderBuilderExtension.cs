using System;
using ManagedCode.Database.EntityFramework.PostgreSQL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Database.EntityFramework.PostgreSQL.Extensions;

public static class ProviderBuilderExtension
{
    public static IServiceCollection AddPostgreSQLBasedOnEF(this IServiceCollection serviceCollection, Action<PostgresConnectionOptions> action)
    {
        var connectionOptions = new PostgresConnectionOptions();
        action.Invoke(connectionOptions);

        serviceCollection.AddDbContext<PostgresDatabaseContext>(options => options
            .UseNpgsql(connectionOptions.ConnectionString)
            .UseQueryTrackingBehavior(
                connectionOptions.UseTracking ? QueryTrackingBehavior.TrackAll : QueryTrackingBehavior.NoTracking
            )
        );

        return serviceCollection;
    }
}