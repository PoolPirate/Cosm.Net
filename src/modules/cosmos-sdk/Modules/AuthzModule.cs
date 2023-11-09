using Cosm.Net.Base;
using Cosmos.Authz.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.Cosmos;
public partial class AuthzModule : IModule<AuthzModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private AuthzModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static AuthzModule IModule<AuthzModule>.FromGrpcChannel(GrpcChannel channel)
        => new AuthzModule(channel);
}