using Cosm.Net.Modules;
using Osmosis.Txfees.V1Beta1;
using Grpc.Net.Client;

namespace Cosm.Net.Osmosis;
internal partial class TxFeesModule : IModule<TxFeesModule, Query.QueryClient>
{
    private readonly Query.QueryClient Service;

    public TxFeesModule(GrpcChannel channel)
    {
        Service = new Query.QueryClient(channel);
    }
}