using Cosm.Net.Modules;
using Osmosis.Gamm.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net;
internal partial class GammModule : IModule<GammModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public GammModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}