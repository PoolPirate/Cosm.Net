using Cosm.Net.Base;
using Cosmos.Feegrant.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.Cosmos;
public partial class FeeGrantModule : IModule<FeeGrantModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private FeeGrantModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static FeeGrantModule IModule<FeeGrantModule>.FromGrpcChannel(GrpcChannel channel)
        => new FeeGrantModule(channel);
}
