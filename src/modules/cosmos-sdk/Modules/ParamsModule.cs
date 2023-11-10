using Cosm.Net.Modules;
using Cosmos.Params.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
public partial class ParamsModule : IModule<ParamsModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    internal ParamsModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
