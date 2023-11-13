using Cosm.Net.Services;
using Cosm.Net.Tx;
using Cosmos.Tx.V1Beta1;


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

    public async Task SimulateTxAsync(ICosmTx tx, ulong sequence)
    {
        var encodedTx = _txEncoder.EncodeTx(tx, sequence);
        await _txModule.SimulateAsync(encodedTx);
    }

    public async Task PublishTxAsync(ISignedCosmTx tx)
    {
        var encodedTx = _txEncoder.EncodeTx(tx);
        var broadcastResponse = await _txModule.BroadcastTxAsync(encodedTx, BroadcastMode.Sync);

    }
}
