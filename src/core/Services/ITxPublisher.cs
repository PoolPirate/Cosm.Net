using Cosm.Net.Models;
using Cosm.Net.Tx;

namespace Cosm.Net.Services;
public interface ITxPublisher
{
    public Task<string> PublishTxAsync(ISignedCosmTx tx);
    public Task<TxSimulation> SimulateTxAsync(ICosmTx tx, ulong sequence);
}

public interface ITxPublisher<TConfiguration> : ITxPublisher;