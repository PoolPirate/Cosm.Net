using Cosm.Net.Core.Tx;
using Cosm.Net.Modules;
using Cosmos.Bank.V1Beta1;
using Cosmos.Base.V1Beta1;
using Cosmos.Tx.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
public partial class BankModule : IModule<BankModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    internal BankModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
