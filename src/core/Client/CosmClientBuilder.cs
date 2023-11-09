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

    public CosmClientBuilder RegisterModule<TModule>()
        where TModule : class, IModule<TModule>
    {
        if(!Services.Any(x => x.ServiceType == typeof(TModule)))
        {
            _ = Services.AddSingleton((provider) =>
            {
                var channel = provider.GetRequiredService<GrpcChannel>();
                return TModule.FromGrpcChannel(channel);
            });
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
