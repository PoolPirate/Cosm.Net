using Cosm.Net.Modules;
using Cosmos.Evidence.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;
public partial class EvidenceModule : IModule<EvidenceModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private EvidenceModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static EvidenceModule IModule<EvidenceModule>.FromGrpcChannel(GrpcChannel channel)
        => new EvidenceModule(channel);
}
