using Cosm.Net.Client.Internal;
using Cosm.Net.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Client;
public class CosmClient : ICosmClient, IInternalCosmClient
{
    private readonly Type[] _moduleTypes;
    private readonly IServiceProvider _serviceProvider;

    IServiceProvider IInternalCosmClient.ServiceProvider 
        => _serviceProvider;

    internal CosmClient(IServiceProvider serviceProvider, IEnumerable<Type> moduleTypes)
    {
        _serviceProvider = serviceProvider;
        _moduleTypes = moduleTypes.ToArray();
    }

    public TModule Module<TModule>() where TModule : IModule
        => _serviceProvider.GetService<TModule>()
            ?? throw new InvalidOperationException("Module not installed!");

    public IEnumerable<(Type, IModule)> GetAllModules()
    {
        foreach(var type in _moduleTypes)
        {
            yield return (type, (IModule) _serviceProvider.GetRequiredService(type));
        }
    }

    public IInternalCosmClient AsInternal() 
        => this;
}
