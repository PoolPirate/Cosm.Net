using Cosm.Net.Modules;
using Grpc.Net.Client;
using Cosmos.Base.Node.V1Beta1;

namespace Cosm.Net.CosmosSdk;

internal partial class NodeService : IModule<NodeService, Service.ServiceClient>
{
    private readonly Service.ServiceClient Service;

    public NodeService(GrpcChannel channel)
    {
        Service = new Service.ServiceClient(channel);
    }
}
