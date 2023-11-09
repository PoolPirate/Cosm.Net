using Cosm.Net.Base;
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

    static ConsensusModule IModule<ConsensusModule>.FromGrpcChannel(GrpcChannel channel)
        => new ConsensusModule(channel);
}
