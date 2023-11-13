using Cosm.Net.Tx;

namespace Cosm.Net.Services;
public interface ITxPublisher
{
    public Task PublishTxAsync(ISignedCosmTx tx);
    public Task SimulateTxAsync(ICosmTx tx, ulong sequence);
}
