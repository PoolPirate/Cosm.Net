using Cosm.Net.Client.Internal;
using Cosm.Net.Models;
using Cosm.Net.Modules;
using Cosm.Net.Services;
using Cosm.Net.Tx;
using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Client;
internal class CosmTxClient : ICosmTxClient, IInternalCosmTxClient
{
    private readonly IServiceProvider _provider;
    private readonly ICosmClient _cosmClient;
    private readonly ITxChainConfiguration _chainConfig;
    private readonly IGasFeeProvider _gasFeeProvider;

    public ITxChainConfiguration Chain => _chainConfig;
    IServiceProvider IInternalCosmClient.ServiceProvider => _cosmClient.AsInternal().ServiceProvider;

    public CosmTxClient(IServiceProvider provider, ICosmClient cosmClient,
        IGasFeeProvider gasFeeProvider, ITxChainConfiguration chainConfig)
    {
        _provider = provider;
        _cosmClient = cosmClient;
        _chainConfig = chainConfig;
        _gasFeeProvider = gasFeeProvider;
    }

    public Task<TxSimulation> SimulateAsync(ICosmTx tx)
    {
        var scheduler = _provider.GetRequiredService<ITxScheduler>();
        return scheduler.SimulateTxAsync(tx);
    }

    public async Task<string> PublishTxAsync(ICosmTx tx, GasFeeAmount gasFee)
    {
        var scheduler = _provider.GetRequiredService<ITxScheduler>();
        return await scheduler.PublishTxAsync(tx, gasFee);
    }
    public async Task<string> SimulateAndPublishTxAsync(ICosmTx tx, decimal gasMultiplier = 1.2m, ulong gasOffset = 20000)
    {
        var chainConfig = _provider.GetRequiredService<ITxChainConfiguration>();
        var simulation = await SimulateAsync(tx);

        ulong gasWanted = (ulong) Math.Ceiling((simulation.GasUsed * gasMultiplier) + gasOffset);
        var fee = await _gasFeeProvider.GetFeeForGasAsync(gasWanted);

        return await PublishTxAsync(tx, fee);
    }

    public TModule Module<TModule>() where TModule : IModule 
        => _cosmClient.Module<TModule>();

    IInternalCosmClient ICosmClient.AsInternal()
        => this;
    public IInternalCosmTxClient AsInternal()
        => this;

    IEnumerable<(Type, IModule)> IInternalCosmClient.GetAllModules() 
        => _cosmClient.AsInternal().GetAllModules();

}
