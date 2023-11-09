using Cosm.Net.Modules;
using Cosmos.Mint.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
public partial class MintModule : IModule<MintModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private MintModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static MintModule IModule<MintModule>.FromGrpcChannel(GrpcChannel channel)
        => new MintModule(channel);
}
