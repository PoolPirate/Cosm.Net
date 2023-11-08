using Cosmos.Gov.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.Client.Modules;
public partial class GovModule : ICosmModule<Query.QueryClient>, IModule<GovModule>
{
    private readonly Query.QueryClient Service;

    private GovModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static GovModule IModule<GovModule>.FromGrpcChannel(GrpcChannel channel)
        => new GovModule(channel);
}