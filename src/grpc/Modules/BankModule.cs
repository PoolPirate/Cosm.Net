using Cosmos.Bank.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.Client.Modules;
public partial class BankModule : ICosmModule<Query.QueryClient>, IModule<BankModule>
{
    private readonly Query.QueryClient Service;

    private BankModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static BankModule IModule<BankModule>.FromGrpcChannel(GrpcChannel channel)
        => new BankModule(channel);
}
