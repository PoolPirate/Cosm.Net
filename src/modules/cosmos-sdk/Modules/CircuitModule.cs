using Cosm.Net.Modules;
using Grpc.Net.Client;
using Cosmos.Circuit.V1;

namespace Cosm.Net.CosmosSdk;
public partial class CircuitModule : IModule<CircuitModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private CircuitModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static CircuitModule IModule<CircuitModule>.FromGrpcChannel(GrpcChannel channel)
        => new CircuitModule(channel);
}
