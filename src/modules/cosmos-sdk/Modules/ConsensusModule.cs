using Cosm.Net.Modules;
using Grpc.Net.Client;
using Cosmos.Consensus.V1;

namespace Cosm.Net.CosmosSdk;
internal partial class ConsensusModule : IModule<ConsensusModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public ConsensusModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
