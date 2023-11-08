using Cosmos.Auth.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.Client.Modules;
public partial class AuthModule : ICosmModule<Query.QueryClient>, IModule<AuthModule>
{
    private readonly Query.QueryClient Service;

    private AuthModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static AuthModule IModule<AuthModule>.FromGrpcChannel(GrpcChannel channel)
        => new AuthModule(channel);
}