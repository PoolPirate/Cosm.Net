using Cosm.Net.Modules;
using Cosmos.Bank.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
internal partial class BankModule : IModule<BankModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public BankModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
