using System;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Database.AzureTables.Extensions;

public static class ProviderBuilderExtension
{
    public static IServiceCollection AddAzureTables(this IServiceCollection serviceCollection,
        Action<AzureTablesOptions> action)
    {
        var connectionOptions = new AzureTablesOptions();
        action.Invoke(connectionOptions);

        serviceCollection.AddSingleton(connectionOptions);

        return serviceCollection;
    }
}