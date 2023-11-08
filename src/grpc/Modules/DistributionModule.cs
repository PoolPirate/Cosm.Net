using Cosmos.Staking.V1Beta1;
using Grpc.Core;
using Grpc.Net.Client;

namespace Cosm.Net.Client.Modules;
public partial class DistributionModule : ICosmModule<Query.QueryClient>, IModule<DistributionModule>
{
    private readonly Query.QueryClient Service;

    private DistributionModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static DistributionModule IModule<DistributionModule>.FromGrpcChannel(GrpcChannel channel)
        => new DistributionModule(channel);
}