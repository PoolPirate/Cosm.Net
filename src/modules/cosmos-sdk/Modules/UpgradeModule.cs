using Cosm.Net.Base;
using Grpc.Net.Client;
using Cosmos.Upgrade.V1Beta1;

namespace Cosm.Net.CosmosSdk;
public partial class UpgradeModule : IModule<UpgradeModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private UpgradeModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static UpgradeModule IModule<UpgradeModule>.FromGrpcChannel(GrpcChannel channel)
        => new UpgradeModule(channel);
}
