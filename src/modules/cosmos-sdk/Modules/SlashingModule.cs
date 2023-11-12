using Cosm.Net.Modules;
using Grpc.Net.Client;
using Cosmos.Slashing.V1Beta1;

namespace Cosm.Net.CosmosSdk;
internal partial class SlashingModule : IModule<SlashingModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public SlashingModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}
