using Cosm.Net.Modules;
using Cosmos.Staking.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
internal partial class StakingModule : IModule<StakingModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public StakingModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}