using Cosmos.Staking.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.Client.Modules;
public partial class StakingModule : ICosmModule<Query.QueryClient>, IModule<StakingModule>
{
    private readonly Query.QueryClient Service;

    private StakingModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static StakingModule IModule<StakingModule>.FromGrpcChannel(GrpcChannel channel)
        => new StakingModule(channel);
}