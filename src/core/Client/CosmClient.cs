using Cosm.Net.Core.Msg;
using Cosm.Net.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Client;
public class CosmClient
{
    private readonly IServiceProvider ModuleProvider;

    internal CosmClient(IServiceProvider moduleProvider)
    {
        ModuleProvider = moduleProvider;
    }

    public TModule Module<TModule>() where TModule : IModule<TModule>
        => ModuleProvider.GetService<TModule>()
            ?? throw new InvalidOperationException("Module not installed!");
}
