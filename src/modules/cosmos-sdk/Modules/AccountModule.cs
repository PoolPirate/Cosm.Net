using Cosm.Net.Modules;
using Cosmos.Accounts.V1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
internal partial class AccountModule : IModule<AccountModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public AccountModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
