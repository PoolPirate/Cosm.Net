using Cosm.Net.Exceptions;
using Cosm.Net.Models;
using Cosm.Net.Services;
using Cosm.Net.Tx;
using Cosmos.Tx.V1Beta1;
using Google.Apis.Auth.OAuth2;

namespace Cosm.Net.CosmosSdk.Tx;
public class TxModulePublisher : ITxPublisher
{
    private readonly ITxModule _txModule;
    private readonly ITxEncoder _txEncoder;

    public TxModulePublisher(ITxModule txModule, ITxEncoder txEncoder)
    {
        _txModule = txModule;
        _txEncoder = txEncoder;
    }

    public async Task<TxSimulation> SimulateTxAsync(ICosmTx tx, ulong sequence)
    {
        var encodedTx = _txEncoder.EncodeTx(tx, sequence);
        var response = await _txModule.SimulateAsync(encodedTx);

        return new TxSimulation(response.GasInfo.GasUsed);
    }

    public async Task<string> PublishTxAsync(ISignedCosmTx tx)
    {
        var encodedTx = _txEncoder.EncodeTx(tx);
        var broadcastResponse = await _txModule.BroadcastTxAsync(encodedTx, BroadcastMode.Sync);

        return broadcastResponse.TxResponse.Code != 0
            ? throw new TxPublishException(broadcastResponse.TxResponse.Code, broadcastResponse.TxResponse.RawLog)
            : broadcastResponse.TxResponse.Txhash;
    }
}
