using Cosm.Net.Modules;
using Grpc.Net.Client;
using Cosmos.Consensus.V1;

namespace Cosm.Net.CosmosSdk;
public partial class ConsensusModule : IModule<ConsensusModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private ConsensusModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
