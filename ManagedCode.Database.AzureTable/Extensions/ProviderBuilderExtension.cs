using System;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Database.AzureTable.Extensions
{
    public static class ProviderBuilderExtension
    {
        public static IServiceCollection AddAzureTable(this IServiceCollection serviceCollection, Action<AzureTableOptions> action)
        {
            var connectionOptions = new AzureTableOptions();
            action.Invoke(connectionOptions);

            serviceCollection.AddSingleton(connectionOptions);

            return serviceCollection;
        }
    }
}