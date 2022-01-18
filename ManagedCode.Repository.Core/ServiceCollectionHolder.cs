using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Repository.Core
{
    public class ServiceCollectionHolder
    {
        public IServiceCollection ServiceCollection { get; private set; }

        public ServiceCollectionHolder(IServiceCollection serviceCollection)
        {
            ServiceCollection = serviceCollection;
        }
    }
}
