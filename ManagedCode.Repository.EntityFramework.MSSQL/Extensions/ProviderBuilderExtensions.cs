using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ManagedCode.Repository.EntityFramework.MSSQL.Models;

namespace ManagedCode.Repository.EntityFramework.MSSQL.Extensions
{
    public static class ProviderBuilderExtensions
    {
        public static IServiceCollection AddMSSQLBasedOnEF(this IServiceCollection serviceCollection, Action<MSSQLConnectionOptions> action)
        {
            var connectionOptions = new MSSQLConnectionOptions();
            action.Invoke(connectionOptions);

            serviceCollection.AddDbContext<MSSQLDatabaseContext>(options => options
               .UseSqlServer(connectionOptions.ConnectionString)
               .UseQueryTrackingBehavior(
                    connectionOptions.UseTracking ? 
                    QueryTrackingBehavior.TrackAll :
                    QueryTrackingBehavior.NoTracking
               )
            );

            return serviceCollection;
        }
    }
}
