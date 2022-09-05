using Microsoft.Extensions.DependencyInjection;

namespace ManagedCode.Database.Core.Extensions;

public class ServiceCollectionHolder
{
    public ServiceCollectionHolder(IServiceCollection serviceCollection)
    {
        ServiceCollection = serviceCollection;
    }

    public IServiceCollection ServiceCollection { get; }
}