using System;
using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Repository.MongoDB.Extensions
{
    public static class ProviderBuilderExtension
    {
        public static IServiceCollection AddMongoDb(this IServiceCollection serviceCollection, Action<MongoDbRepositoryOptions> action)
        {
            var connectionOptions = new MongoDbRepositoryOptions();
            action.Invoke(connectionOptions);

            serviceCollection.AddSingleton(connectionOptions);

            return serviceCollection;
        }
    }
}
