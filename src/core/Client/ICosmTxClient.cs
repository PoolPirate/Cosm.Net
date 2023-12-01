using Cosm.Net.Client.Internal;
using Cosm.Net.Models;
using Cosm.Net.Tx;

namespace Cosm.Net.Client;
public interface ICosmTxClient : ICosmClient
{
    public ITxChainConfiguration Chain {  get; }

    public Task<TxSimulation> SimulateAsync(ICosmTx tx);
    public Task<string> PublishTxAsync(ICosmTx tx, GasFeeAmount gasFee);
    public Task<string> PublishTxAsync(ICosmTx tx, ulong gasWanted);
    public Task<string> SimulateAndPublishTxAsync(ICosmTx tx, decimal gasMultiplier = 1.2m, ulong gasOffset = 20000);

    public new IInternalCosmTxClient AsInternal();
}
