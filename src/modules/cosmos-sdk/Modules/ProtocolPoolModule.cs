using Cosm.Net.Modules;
using Grpc.Net.Client;
using Cosmos.Protocolpool.V1;

namespace Cosm.Net.CosmosSdk;
public partial class ProtocolPoolModule : IModule<ProtocolPoolModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private ProtocolPoolModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static ProtocolPoolModule IModule<ProtocolPoolModule>.FromGrpcChannel(GrpcChannel channel)
        => new ProtocolPoolModule(channel);
}
