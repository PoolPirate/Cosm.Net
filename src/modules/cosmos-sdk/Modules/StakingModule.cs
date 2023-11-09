using Cosm.Net.Base;
using Cosmos.Staking.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.Cosmos;
public partial class StakingModule : IModule<StakingModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private StakingModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static StakingModule IModule<StakingModule>.FromGrpcChannel(GrpcChannel channel)
        => new StakingModule(channel);
}