using Cosm.Net.Base;
using Cosmos.Nft.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.Cosmos;
public partial class NftModule : IModule<NftModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private NftModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static NftModule IModule<NftModule>.FromGrpcChannel(GrpcChannel channel)
        => new NftModule(channel);
}
