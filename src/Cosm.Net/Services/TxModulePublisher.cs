using Cosm.Net.Adapters.Internal;
using Cosm.Net.Models;
using Cosm.Net.Models.Tx;
using Cosm.Net.Tx;
using Grpc.Core;

namespace Cosm.Net.Services;
public class TxModulePublisher(IInternalTxAdapter txModule, ITxEncoder txEncoder)
    : ITxPublisher
{
    private readonly IInternalTxAdapter _txModule = txModule;
    private readonly ITxEncoder _txEncoder = txEncoder;

    public async Task<TxSubmission> PublishTxAsync(ISignedCosmTx tx, DateTime? deadline, CancellationToken cancellationToken)
    {
        try
        {
            return await _txModule.BroadcastTxAsync(
                _txEncoder.EncodeTx(tx),
                BroadcastMode.Sync,
                deadline: deadline,
                cancellationToken: cancellationToken
            );
        }
        catch(RpcException ex) when(ex.StatusCode == StatusCode.Unknown && ex.Message.Contains("sequence mismatch"))
        {
            int txHashStartIdx = ex.Message.IndexOf("tx ") + 3;
            return new TxSubmission(32, ex.Message.Substring(txHashStartIdx, 64), ex.Message);
        }
    }
}
