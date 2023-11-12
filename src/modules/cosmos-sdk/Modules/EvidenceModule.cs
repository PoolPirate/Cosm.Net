using Cosm.Net.Modules;
using Cosmos.Evidence.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
internal partial class EvidenceModule : IModule<EvidenceModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public EvidenceModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
