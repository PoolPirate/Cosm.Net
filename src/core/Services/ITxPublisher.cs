using Cosm.Net.Models;
using Cosm.Net.Tx;

namespace Cosm.Net.Services;
public interface ITxPublisher
{
    public Task PublishTxAsync(ISignedCosmTx tx);
    public Task<TxSimulation> SimulateTxAsync(ICosmTx tx, ulong sequence);
}
