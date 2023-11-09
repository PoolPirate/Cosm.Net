using Cosm.Net.Base;
using Cosmos.Tx.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.Cosmos;
public partial class TxModule : IModule<TxModule, Service.ServiceClient>
{
    private readonly Service.ServiceClient Service;

    private TxModule(GrpcChannel channel)
    {
        Service = new Service.ServiceClient(channel);
    }

    static TxModule IModule<TxModule>.FromGrpcChannel(GrpcChannel channel)
        => new TxModule(channel);
}

