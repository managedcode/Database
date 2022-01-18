using Microsoft.Extensions.DependencyInjection;
using System;

namespace ManagedCode.Repository.SQLite.Extensions
{
    public static class ProviderBuilderExtension
    {
        public static IServiceCollection AddSQLite(this IServiceCollection serviceCollection, Action<SQLiteRepositoryOptions> action)
        {
            var connectionOptions = new SQLiteRepositoryOptions();
            action.Invoke(connectionOptions);

            serviceCollection.AddSingleton(connectionOptions);

            return serviceCollection;
        }
    }
}
