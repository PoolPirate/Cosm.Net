using Cosmos.Authz.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.Client.Modules;
public partial class AuthzModule : ICosmModule<Query.QueryClient>, IModule<AuthzModule>
{
    private readonly Query.QueryClient Service;

    private AuthzModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static AuthzModule IModule<AuthzModule>.FromGrpcChannel(GrpcChannel channel)
        => new AuthzModule(channel);
}