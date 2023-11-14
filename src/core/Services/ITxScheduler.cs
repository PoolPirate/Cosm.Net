using Cosm.Net.Models;
using Cosm.Net.Tx;

namespace Cosm.Net.Services;
public interface ITxScheduler
{   
    public Task<TxSimulation> SimulateTxAsync(ICosmTx tx);
    public Task<string> PublishTxAsync(ICosmTx tx, ulong gasWanted, string feeDenom, ulong feeAmount);

    public Task InitializeAsync();
}
