using Cosm.Net.Modules;
using Cosmos.Feegrant.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
public partial class FeeGrantModule : IModule<FeeGrantModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private FeeGrantModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
