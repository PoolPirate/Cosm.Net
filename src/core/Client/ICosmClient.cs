using Cosm.Net.Modules;

namespace Cosm.Net.Client;
public interface ICosmClient
{
    public TModule Module<TModule>() where TModule : IModule;

    internal IEnumerable<(Type, IModule)> GetAllModules();
}
