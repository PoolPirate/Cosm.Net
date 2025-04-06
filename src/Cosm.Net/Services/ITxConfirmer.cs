using Cosm.Net.Models;
using Cosm.Net.Tx;

namespace Cosm.Net.Services;
public interface ITxConfirmer
{
    Task<TxExecution> WaitForTxConfirmationAsync(string txHash, TimeSpan timeout, bool throwOnRevert = true, CancellationToken cancellationToken = default);

    public void Initialize(IChainConfiguration chainConfiguration);
}

public interface ITxConfirmer<TConfiguration> : ITxConfirmer;