using System;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Repository.CosmosDB.Extensions;

public static class ProviderBuilderExtension
{
    public static IServiceCollection AddCosmosDb(this IServiceCollection serviceCollection, Action<CosmosDbRepositoryOptions> action)
    {
        var connectionOptions = new CosmosDbRepositoryOptions();
        action.Invoke(connectionOptions);

        serviceCollection.AddSingleton(connectionOptions);

        return serviceCollection;
    }
}