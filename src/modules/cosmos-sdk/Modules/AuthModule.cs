using Cosm.Net.Modules;
using Cosmos.Auth.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
internal partial class AuthModule : IModule<AuthModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public AuthModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}