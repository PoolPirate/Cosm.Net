using Cosm.Net.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Client;
public class CosmClient : ICosmClient
{
    private readonly Type[] _moduleTypes;
    private readonly IServiceProvider _moduleProvider;

    internal CosmClient(IServiceProvider moduleProvider, IEnumerable<Type> moduleTypes)
    {
        _moduleProvider = moduleProvider;
        _moduleTypes = moduleTypes.ToArray();
    }

    public TModule Module<TModule>() where TModule : IModule
        => _moduleProvider.GetService<TModule>()
            ?? throw new InvalidOperationException("Module not installed!");

    public IEnumerable<(Type, IModule)> GetAllModules()
    {
        foreach(var type in _moduleTypes)
        {
            yield return (type, (IModule) _moduleProvider.GetRequiredService(type));
        }
    }
}
