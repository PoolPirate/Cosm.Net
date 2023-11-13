using Cosm.Net.Modules;
using Cosm.Net.Tx;
using Cosmos.Tx.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
internal partial class TxModule : IModule<TxModule, Service.ServiceClient>
{
    private readonly Service.ServiceClient Service;

    public TxModule(GrpcChannel channel)
    {
        Service = new Service.ServiceClient(channel);
    }
}

