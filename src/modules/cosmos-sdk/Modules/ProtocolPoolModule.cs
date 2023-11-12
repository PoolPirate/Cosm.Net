using Cosm.Net.Modules;
using Grpc.Net.Client;
using Cosmos.Protocolpool.V1;

namespace Cosm.Net.CosmosSdk;
internal partial class ProtocolPoolModule : IModule<ProtocolPoolModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public ProtocolPoolModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
