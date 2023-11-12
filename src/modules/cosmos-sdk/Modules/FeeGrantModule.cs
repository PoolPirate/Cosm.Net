using Cosm.Net.Modules;
using Cosmos.Feegrant.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
internal partial class FeeGrantModule : IModule<FeeGrantModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public FeeGrantModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
