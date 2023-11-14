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

    public async Task<string> PublishTxAsync(ICosmTx tx, ulong gasWanted, string feeDenom, ulong feeAmount)
    {
        var scheduler = _provider.GetRequiredService<ITxScheduler>();
        return await scheduler.PublishTxAsync(tx, gasWanted, feeDenom, feeAmount);
    }
    public async Task<string> SimulateAndPublishTxAsync(ICosmTx tx, decimal gasMultiplier = 1.2m, ulong gasOffset = 20000)
    {
        var chainConfig = _provider.GetRequiredService<ITxChainConfiguration>();
        var simulation = await SimulateAsync(tx);

        ulong gasWanted = (ulong) Math.Ceiling((simulation.GasUsed * gasMultiplier) + gasOffset);

        return await PublishTxAsync(tx, gasWanted, chainConfig.FeeDenom, (ulong) Math.Ceiling(chainConfig.GasPrice * gasWanted));
    }
}
