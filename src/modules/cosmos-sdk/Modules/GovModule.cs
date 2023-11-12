using Cosm.Net.Modules;
using Cosmos.Gov.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
internal partial class GovModule : IModule<GovModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public GovModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}