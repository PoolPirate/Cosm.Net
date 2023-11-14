using Grpc.Net.Client;
using Cosmos.Base.Tendermint.V1Beta1;
using Cosm.Net.Modules;

namespace Cosm.Net.CosmosSdk;
internal partial class TendermintService : IModule<TendermintService, Service.ServiceClient>
{
    private readonly Service.ServiceClient Service;

    public TendermintService(GrpcChannel channel)
    {
        Service = new Service.ServiceClient(channel);
    }
}
