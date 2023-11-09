using Cosm.Net.Base;
using Cosmos.Accounts.V1;
using Grpc.Net.Client;

namespace Cosm.Net.Cosmos;
public partial class AccountModule : IModule<AccountModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private AccountModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static AccountModule IModule<AccountModule>.FromGrpcChannel(GrpcChannel channel)
        => new AccountModule(channel);
}
