using Cosm.Net.Adapters;
using Cosm.Net.Exceptions;
using Cosm.Net.Models;
using Cosm.Net.Tx;

namespace Cosm.Net.Services;
public class TxModulePublisher(ITxModuleAdapter txModule, ITxEncoder txEncoder, IGasFeeProvider gasFeeProvider) 
    : ITxPublisher
{
    private readonly ITxModuleAdapter _txModule = txModule;
    private readonly ITxEncoder _txEncoder = txEncoder;
    private readonly IGasFeeProvider _gasFeeProvider = gasFeeProvider;

    public async Task<TxSimulation> SimulateTxAsync(ICosmTx tx, ulong sequence)
    {
        var encodedTx = _txEncoder.EncodeTx(tx, sequence, _gasFeeProvider.BaseGasFeeDenom);
       return await _txModule.SimulateAsync(encodedTx);
    }

    public async Task<string> PublishTxAsync(ISignedCosmTx tx)
    {
        var encodedTx = _txEncoder.EncodeTx(tx);
        var broadcastResponse = await _txModule.BroadcastTxAsync(encodedTx, Cosm.Net.Models.BroadcastMode.Sync);

        return broadcastResponse.Code != 0
            ? throw new TxPublishException(broadcastResponse.Code, broadcastResponse.RawLog)
            : broadcastResponse.TxHash;
    }
}
