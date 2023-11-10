using Cosm.Net.Modules;
using Cosmos.Staking.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
public partial class StakingModule : IModule<StakingModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private StakingModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}