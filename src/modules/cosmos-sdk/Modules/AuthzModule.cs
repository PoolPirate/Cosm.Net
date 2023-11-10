using Cosm.Net.Modules;
using Cosmos.Authz.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
public partial class AuthzModule : IModule<AuthzModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    internal AuthzModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}