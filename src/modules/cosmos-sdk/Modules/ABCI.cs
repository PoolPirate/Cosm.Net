using Cosm.Net.Modules;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;

internal partial class ABCI : IModule<ABCI, Tendermint.Abci.ABCI.ABCIClient>
{
    private readonly Tendermint.Abci.ABCI.ABCIClient Service;

    public ABCI(GrpcChannel channel)
    {
        Service = new Tendermint.Abci.ABCI.ABCIClient(channel);
    }
}
