using Cosm.Net.Modules;
using Cosmos.Mint.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
internal partial class MintModule : IModule<MintModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public MintModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
