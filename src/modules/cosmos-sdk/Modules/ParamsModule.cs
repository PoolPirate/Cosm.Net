using Cosm.Net.Modules;
using Cosmos.Params.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
internal partial class ParamsModule : IModule<ParamsModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public ParamsModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
