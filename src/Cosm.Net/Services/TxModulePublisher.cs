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

    public async Task<TxSubmission> PublishTxAsync(ISignedCosmTx tx, DateTime? deadline, CancellationToken cancellationToken)
    {
        var encodedTx = _txEncoder.EncodeTx(tx);
        return await _txModule.BroadcastTxAsync(encodedTx, BroadcastMode.Sync, 
            deadline: deadline, cancellationToken: cancellationToken);
    }
}
