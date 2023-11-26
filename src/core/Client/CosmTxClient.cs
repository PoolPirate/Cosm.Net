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

    public ITxChainConfiguration Chain => _chainConfig;
    IServiceProvider IInternalCosmClient.ServiceProvider => _cosmClient.AsInternal().ServiceProvider;

    public CosmTxClient(IServiceProvider provider, ICosmClient cosmClient, ITxChainConfiguration chainConfig)
    {
        _provider = provider;
        _cosmClient = cosmClient;
        _chainConfig = chainConfig;
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

    public TModule Module<TModule>() where TModule : IModule 
        => _cosmClient.Module<TModule>();

    IInternalCosmClient ICosmClient.AsInternal()
        => this;
    public IInternalCosmTxClient AsInternal()
        => this;

    IEnumerable<(Type, IModule)> IInternalCosmClient.GetAllModules() 
        => _cosmClient.AsInternal().GetAllModules();

}
