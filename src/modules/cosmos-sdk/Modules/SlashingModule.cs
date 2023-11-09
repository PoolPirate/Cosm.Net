using Cosm.Net.Modules;
using Grpc.Net.Client;
using Cosmos.Slashing.V1Beta1;

namespace Cosm.Net.CosmosSdk;
public partial class SlashingModule : IModule<SlashingModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    private SlashingModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }

    static SlashingModule IModule<SlashingModule>.FromGrpcChannel(GrpcChannel channel)
        => new SlashingModule(channel);
}
