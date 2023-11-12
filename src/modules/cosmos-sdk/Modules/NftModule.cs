using Cosm.Net.Modules;
using Cosmos.Nft.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
internal partial class NftModule : IModule<NftModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public NftModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
