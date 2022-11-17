using System;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Database.SQLite.Extensions;

public static class ProviderBuilderExtension
{
    public static IServiceCollection AddSQLite(this IServiceCollection serviceCollection,
        Action<SQLiteRepositoryOptions> action)
    {
        var connectionOptions = new SQLiteRepositoryOptions();
        action.Invoke(connectionOptions);

        serviceCollection.AddSingleton(connectionOptions);

        return serviceCollection;
    }
}