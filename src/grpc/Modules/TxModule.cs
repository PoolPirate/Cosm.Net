using Cosmos.Tx.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.Client.Modules;
public partial class TxModule : ICosmModule<Service.ServiceClient>, IModule<TxModule>
{
    private readonly Service.ServiceClient Service;

    private TxModule(GrpcChannel channel)
    {
        Service = new Service.ServiceClient(channel);
    }

    static TxModule IModule<TxModule>.FromGrpcChannel(GrpcChannel channel)
        => new TxModule(channel);
}
