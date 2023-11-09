using Cosm.Net.Modules;
using Cosmos.Bank.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
public partial class BankModule : IModule<BankModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private BankModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static BankModule IModule<BankModule>.FromGrpcChannel(GrpcChannel channel)
        => new BankModule(channel);
}
