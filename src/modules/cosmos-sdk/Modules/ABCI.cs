using Cosm.Net.Modules;
using Grpc.Net.Client;

namespace Cosm.Net.CosmosSdk;

public partial class ABCI : IModule<ABCI, Tendermint.Abci.ABCI.ABCIClient>
{
    private readonly Tendermint.Abci.ABCI.ABCIClient Service;

    private ABCI(GrpcChannel channel)
    {
        Service = new Tendermint.Abci.ABCI.ABCIClient(channel);
    }
}
