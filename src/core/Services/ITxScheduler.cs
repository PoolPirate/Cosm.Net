using Cosm.Net.Models;
using Cosm.Net.Tx;

namespace Cosm.Net.Services;
public interface ITxScheduler
{
    public ValueTask QueueTxAsync(ICosmTx tx, ulong gasWanted, string feeDenom, ulong feeAmount);
    public Task<TxSimulation> SimulateTxAsync(ICosmTx tx);
    public Task InitializeAsync();
}
