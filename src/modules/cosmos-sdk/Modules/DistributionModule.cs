using Cosm.Net.Modules;
using Cosmos.Staking.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
internal partial class DistributionModule : IModule<DistributionModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public DistributionModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}