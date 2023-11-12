using Cosm.Net.Modules;
using Grpc.Net.Client;
using Cosmos.Upgrade.V1Beta1;

namespace Cosm.Net.CosmosSdk;
internal partial class UpgradeModule : IModule<UpgradeModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public UpgradeModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
