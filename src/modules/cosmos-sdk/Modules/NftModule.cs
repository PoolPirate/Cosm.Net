using Cosm.Net.Modules;
using Cosmos.Nft.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
public partial class NftModule : IModule<NftModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private NftModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
