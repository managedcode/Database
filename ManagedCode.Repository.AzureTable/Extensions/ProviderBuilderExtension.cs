using System;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Repository.AzureTable.Extensions;

public static class ProviderBuilderExtension
{
    public static IServiceCollection AddAzureTable(this IServiceCollection serviceCollection, Action<AzureTableRepositoryOptions> action)
    {
        var connectionOptions = new AzureTableRepositoryOptions();
        action.Invoke(connectionOptions);

        serviceCollection.AddSingleton(connectionOptions);

        return serviceCollection;
    }
}