using System;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Database.Cosmos.Extensions;

public static class ProviderBuilderExtension
{
    public static IServiceCollection AddCosmosDb(this IServiceCollection serviceCollection,
        Action<CosmosOptions> action)
    {
        var connectionOptions = new CosmosOptions();
        action.Invoke(connectionOptions);

        serviceCollection.AddSingleton(connectionOptions);

        return serviceCollection;
    }
}