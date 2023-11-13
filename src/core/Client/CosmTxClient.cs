﻿using Cosm.Net.Services;
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

    public async Task SimulateAsync(ICosmTx tx)
    {
        var scheduler = _provider.GetRequiredService<ITxScheduler>();
        await scheduler.SimulateTxAsync(tx);
    }

    public async Task PublishTxAsync(ICosmTx tx)
    {
        var scheduler = _provider.GetRequiredService<ITxScheduler>();
        await scheduler.QueueTxAsync(tx);
    }
}