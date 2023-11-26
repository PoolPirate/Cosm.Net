using Cosm.Net.Modules;

namespace Cosm.Net.Client.Internal;
public interface IInternalCosmClient
{
    public IServiceProvider ServiceProvider { get; }

    public IEnumerable<(Type, IModule)> GetAllModules();
}
