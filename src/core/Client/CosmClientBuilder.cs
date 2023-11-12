using Cosm.Net.Modules;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Client;
public sealed class CosmClientBuilder
{
    private readonly ServiceCollection Services = new ServiceCollection();

    public CosmClientBuilder WithChannel(GrpcChannel channel)
    {
        if(Services.Any(x => x.ServiceType == typeof(GrpcChannel)))
        {
            throw new InvalidOperationException("Channel already set");
        }

        _ = Services.AddSingleton(channel);
        return this;
    }

    public CosmClientBuilder RegisterModule<TIModule, TModule>()
        where TModule : class, IModule, TIModule
        where TIModule : class, IModule
    {
        if(!Services.Any(x => x.ServiceType == typeof(TModule)))
        {
            _ = Services.AddSingleton<TIModule, TModule>();
        }

        return this;
    }

    public CosmClient Build()
    {
        if(!Services.Any(x => x.ServiceType == typeof(GrpcChannel)))
        {
            throw new InvalidOperationException("No channel set!");
        }

        var moduleProvider = Services.BuildServiceProvider();

        var client = new CosmClient(moduleProvider);
        return client;
    }
}
