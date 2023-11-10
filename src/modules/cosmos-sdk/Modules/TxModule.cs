using Cosm.Net.Modules;
using Cosmos.Tx.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
public partial class TxModule : IModule<TxModule, Service.ServiceClient>
{
    private readonly Service.ServiceClient Service;

    private TxModule(GrpcChannel channel)
    {
        Service = new Service.ServiceClient(channel);
    }
}

