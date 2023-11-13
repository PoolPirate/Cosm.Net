using Cosm.Net.Modules;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Client;
public sealed class CosmClientBuilder
{
    private readonly ServiceCollection _services = new ServiceCollection();
    private readonly List<Type> _moduleTypes = new List<Type>();

    public CosmClientBuilder WithChannel(GrpcChannel channel)
    {
        if(_services.Any(x => x.ServiceType == typeof(GrpcChannel)))
        {
            throw new InvalidOperationException("Channel already set");
        }

        _ = _services.AddSingleton(channel);
        return this;
    }

    public CosmClientBuilder RegisterModule<TIModule, TModule>()
        where TModule : class, IModule, TIModule
        where TIModule : class, IModule
    {
        if(!_services.Any(x => x.ServiceType == typeof(TModule)))
        {
            _ = _services.AddSingleton<TIModule, TModule>();
            _moduleTypes.Add(typeof(TIModule));
        }

        return this;
    }

    public CosmClient Build()
    {
        if(!_services.Any(x => x.ServiceType == typeof(GrpcChannel)))
        {
            throw new InvalidOperationException("No channel set!");
        }

        var moduleProvider = _services.BuildServiceProvider();

        var client = new CosmClient(moduleProvider, _moduleTypes);
        return client;
    }
}
