using System;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Database.LiteDB.Extensions;

public static class ProviderBuilderExtension
{
    public static IServiceCollection AddLiteDb(this IServiceCollection serviceCollection, Action<LiteDBOptions> action)
    {
        var connectionOptions = new LiteDBOptions();
        action.Invoke(connectionOptions);

        serviceCollection.AddSingleton(connectionOptions);

        return serviceCollection;
    }
}