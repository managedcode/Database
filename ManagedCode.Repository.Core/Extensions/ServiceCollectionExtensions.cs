using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Repository.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddManagedCodeRepository(this IServiceCollection serviceCollection)
        {
            return serviceCollection;
        }
    }
}
