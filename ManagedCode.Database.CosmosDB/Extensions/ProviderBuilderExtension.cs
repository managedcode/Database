using System;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Database.CosmosDB.Extensions;

public static class ProviderBuilderExtension
{
    public static IServiceCollection AddCosmosDb(this IServiceCollection serviceCollection, Action<CosmosDBRepositoryOptions> action)
    {
        var connectionOptions = new CosmosDBRepositoryOptions();
        action.Invoke(connectionOptions);

        serviceCollection.AddSingleton(connectionOptions);

        return serviceCollection;
    }
}