using Cosm.Net.Models;
using Cosm.Net.Services;
using Cosm.Net.Tx;
using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Client;
public class CosmTxClient
{
    private readonly IServiceProvider _provider;

    public CosmTxClient(IServiceProvider provider)
    {
        _provider = provider;
    }

    public Task<TxSimulation> SimulateAsync(ICosmTx tx)
    {
        var scheduler = _provider.GetRequiredService<ITxScheduler>();
        return scheduler.SimulateTxAsync(tx);
    }

    public async Task PublishTxAsync(ICosmTx tx, ulong gasWanted, string feeDenom, ulong feeAmount)
    {
        var scheduler = _provider.GetRequiredService<ITxScheduler>();
        await scheduler.QueueTxAsync(tx, gasWanted, feeDenom, feeAmount);
    }

    public async Task SimulateAndPublishTxAsync(ICosmTx tx)
    {
        var chainConfig = _provider.GetRequiredService<ITxChainConfiguration>();
        var simulation = await SimulateAsync(tx);

        ulong gasWanted = (ulong) Math.Ceiling((simulation.GasUsed * 1.2) + 20000);

        await PublishTxAsync(tx, gasWanted, chainConfig.FeeDenom, (ulong) Math.Ceiling(chainConfig.GasPrice * gasWanted));
    }
}
