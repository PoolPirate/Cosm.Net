using Cosm.Net.Adapters;
using Cosm.Net.Models;
using Cosm.Net.Tx;

namespace Cosm.Net.Services;
public class TxModulePublisher(ITxModuleAdapter txModule, ITxEncoder txEncoder)
    : ITxPublisher
{
    private readonly ITxModuleAdapter _txModule = txModule;
    private readonly ITxEncoder _txEncoder = txEncoder;

    public async Task<TxSubmission> PublishTxAsync(ISignedCosmTx tx, DateTime? deadline, CancellationToken cancellationToken) 
        => await _txModule.BroadcastTxAsync(
            _txEncoder.EncodeTx(tx), 
            BroadcastMode.Sync, 
            deadline: deadline, 
            cancellationToken: cancellationToken
        );
}
