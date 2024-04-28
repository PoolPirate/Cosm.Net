using Cosm.Net.Models;
using Cosm.Net.Tx;

namespace Cosm.Net.Services;
public interface ITxScheduler
{
    public Task<TxSimulation> SimulateTxAsync(ICosmTx tx, CancellationToken cancellationToken = default);
    public Task<string> PublishTxAsync(ICosmTx tx, GasFeeAmount gasFee, 
        DateTime? deadline = default, CancellationToken cancellationToken = default);

    public Task InitializeAsync(CancellationToken cancellationToken = default);
}

public interface ITxScheduler<TConfiguration> : ITxScheduler;