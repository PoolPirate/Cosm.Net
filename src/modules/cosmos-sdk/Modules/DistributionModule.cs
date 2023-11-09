using Cosm.Net.Base;
using Cosmos.Staking.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.Cosmos;
public partial class DistributionModule : IModule<DistributionModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private DistributionModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static DistributionModule IModule<DistributionModule>.FromGrpcChannel(GrpcChannel channel)
        => new DistributionModule(channel);
}