using Cosm.Net.Models;
using Cosm.Net.Exceptions;
using Cosm.Net.Tx;
using Cosm.Net.Adapters;
using Grpc.Core;

namespace Cosm.Net.Services;
public class PollingTxConfirmer : ITxConfirmer
{
    private readonly ITxModuleAdapter _txModuleAdapater;
    private IChainConfiguration _chainConfiguration;

    public PollingTxConfirmer(ITxModuleAdapter txModuleAdapater)
    {
        _txModuleAdapater = txModuleAdapater;
        _chainConfiguration = null!;
    }

    public void Initialize(IChainConfiguration chainConfiguration)
    {
        _chainConfiguration = chainConfiguration;
    }

    public async Task<TxExecution> WaitForTxConfirmationAsync(string txHash, 
        TimeSpan timeout, bool throwOnRevert = true, CancellationToken cancellationToken = default)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(4));
        var timeoutTimestamp = DateTimeOffset.UtcNow + timeout;

        while(await timer.WaitForNextTickAsync(cancellationToken))
        {
            if(DateTimeOffset.UtcNow > timeoutTimestamp)
            {
                break;
            }

            try
            {
                var txExecution = await _txModuleAdapater.GetTxByHashAsync(txHash, cancellationToken: cancellationToken);

                if(txExecution is not null)
                {
                    return !txExecution.Success && throwOnRevert
                        ? throw new TxRevertedException(_chainConfiguration.ChainId, txHash)
                        : txExecution;
                }
            }
            catch(RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                continue;
            }
        }

        throw new TimeoutException($"Transaction {txHash} not confirmed on {_chainConfiguration.ChainId} after {timeout}");
    }
}
