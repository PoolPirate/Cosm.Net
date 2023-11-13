using Cosm.Net.Tx;

namespace Cosm.Net.Services;
public interface ITxScheduler
{
    public ValueTask QueueTxAsync(ICosmTx tx);
    public Task SimulateTxAsync(ICosmTx tx);
    public Task InitializeAsync();
}
