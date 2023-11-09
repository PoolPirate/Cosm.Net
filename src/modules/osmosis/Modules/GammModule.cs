using Cosm.Net.Base;
using Osmosis.Gamm.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net;
public partial class GammModule : IModule<GammModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private GammModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static GammModule IModule<GammModule>.FromGrpcChannel(GrpcChannel channel)
        => new GammModule(channel);
}