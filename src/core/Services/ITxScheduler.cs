using Cosm.Net.Models;
using Cosm.Net.Tx;

namespace Cosm.Net.Services;
public interface ITxScheduler
{
    public Task<TxSimulation> SimulateTxAsync(ICosmTx tx);
    public Task<string> PublishTxAsync(ICosmTx tx, GasFeeAmount gasFee);

    public Task InitializeAsync();
}

public interface ITxScheduler<TConfiguration> : ITxScheduler;