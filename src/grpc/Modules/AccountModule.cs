using Cosmos.Accounts.V1;
using Grpc.Net.Client;

namespace Cosm.Net.Client.Modules;
public partial class AccountModule : ICosmModule<Query.QueryClient>, IModule<AccountModule>
{
    private readonly Query.QueryClient Service;

    private AccountModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static AccountModule IModule<AccountModule>.FromGrpcChannel(GrpcChannel channel)
        => new AccountModule(channel);
}
