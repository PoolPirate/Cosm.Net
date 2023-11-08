using Cosmos.Nft.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.Client.Modules;
public partial class NftModule : ICosmModule<Query.QueryClient>, IModule<NftModule>
{
    private readonly Query.QueryClient Service;

    private NftModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static NftModule IModule<NftModule>.FromGrpcChannel(GrpcChannel channel)
        => new NftModule(channel);
}
