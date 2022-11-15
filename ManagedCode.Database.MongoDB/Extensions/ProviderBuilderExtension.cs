using System;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Database.MongoDB.Extensions;

public static class ProviderBuilderExtension
{
    public static IServiceCollection AddMongoDb(this IServiceCollection serviceCollection, Action<MongoOptions> action)
    {
        var connectionOptions = new MongoOptions();
        action.Invoke(connectionOptions);

        serviceCollection.AddSingleton(connectionOptions);

        return serviceCollection;
    }
}