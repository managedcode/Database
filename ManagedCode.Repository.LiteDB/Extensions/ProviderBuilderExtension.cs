using System;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Repository.LiteDB.Extensions
{
    public static class ProviderBuilderExtension
    {
        public static IServiceCollection AddLiteDb(this IServiceCollection serviceCollection, Action<LiteDbRepositoryOptions> action)
        {
            var connectionOptions = new LiteDbRepositoryOptions();
            action.Invoke(connectionOptions);

            serviceCollection.AddSingleton(connectionOptions);

            return serviceCollection;
        }
    }
}
