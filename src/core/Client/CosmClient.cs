﻿using Cosm.Net.Adapters;
using Cosm.Net.Client.Internal;
using Cosm.Net.Models;
using Cosm.Net.Modules;
using Cosm.Net.Services;
using Cosm.Net.Signer;
using Cosm.Net.Tx;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Client;
internal class CosmClient : ICosmTxClient, IInternalCosmTxClient
{
    private readonly Type[] _moduleTypes;
    private readonly IServiceProvider _provider;
    private readonly ChainConfiguration _chainConfig;

    private bool _isInitialized = false;

    private readonly bool _isTxClient;
    private readonly IGasFeeProvider? _gasFeeProvider;

    public IChainConfiguration Chain => _chainConfig;
    IServiceProvider IInternalCosmClient.ServiceProvider => _provider;

    internal CosmClient(IServiceProvider provider, IEnumerable<Type> moduleTypes, ChainConfiguration chainConfiguration, bool isTxClient)
    {
        _provider = provider;
        _moduleTypes = moduleTypes.ToArray();
        _chainConfig = chainConfiguration;

        _isTxClient = isTxClient;

        if(_isTxClient)
        {
            _gasFeeProvider = _provider.GetRequiredService<IGasFeeProvider>();
        }
    }

    public async Task InitializeAsync()
    {
        var tendermintAdapter = _provider.GetRequiredService<ITendermintModuleAdapter>();

        string chainId = await tendermintAdapter.GetChainId();
        _chainConfig.Initialize(chainId);

        if(_isTxClient)
        {
            await InitializeTxClientAync();
        }

        var initializeables = _provider.GetServices<IInitializeableService>();

        await Task.WhenAll(initializeables.Select(
            async initializeable => await initializeable.InitializeAsync()));

        _isInitialized = true;
    }

    private async Task InitializeTxClientAync()
    {
        var txScheduler = _provider.GetRequiredService<ITxScheduler>();

        try
        {
            await txScheduler.InitializeAsync();
        }
        catch(RpcException e)
            when(e.StatusCode == StatusCode.NotFound && e.Status.Detail.StartsWith("account") && e.Status.Detail.EndsWith("not found"))
        {
            var signer = _provider.GetRequiredService<IOfflineSigner>();
            throw new Exception($"Your account {signer.GetAddress(Chain.Bech32Prefix)} was not found on chain. " +
                "Cosmos chains create accounts when they receive funds for the first time.");
        }
    }

    private void AssertReady(bool requireTxClient)
    {
        if(!_isInitialized)
        {
            throw new InvalidOperationException($"Client not initialized. Call {nameof(ICosmClient)}.{nameof(ICosmClient.InitializeAsync)} before using it.");
        }
        if(!_isTxClient && requireTxClient)
        {
            throw new InvalidOperationException($"Method can only be called on {nameof(ICosmTxClient)}");
        }
    }

    public Task<TxSimulation> SimulateAsync(ICosmTx tx)
    {
        AssertReady(true);
        var scheduler = _provider.GetRequiredService<ITxScheduler>();
        return scheduler.SimulateTxAsync(tx);
    }

    public async Task<string> PublishTxAsync(ICosmTx tx, GasFeeAmount gasFee, 
        DateTime? deadline = default, CancellationToken cancellationToken = default)
    {
        AssertReady(true);
        var scheduler = _provider.GetRequiredService<ITxScheduler>();
        return await scheduler.PublishTxAsync(tx, gasFee, deadline, cancellationToken);
    }
    public async Task<string> PublishTxAsync(ICosmTx tx, ulong gasWanted,
        DateTime? deadline = default, CancellationToken cancellationToken = default)
    {
        AssertReady(true);
        var fee = await _gasFeeProvider!.GetFeeForGasAsync(gasWanted);
        return await PublishTxAsync(tx, fee, deadline, cancellationToken);
    }
    public async Task<string> SimulateAndPublishTxAsync(ICosmTx tx, decimal gasMultiplier = 1.2m, ulong gasOffset = 20000, 
        DateTime? deadline = default, CancellationToken cancellationToken = default)
    {
        AssertReady(true);
        var simulation = await SimulateAsync(tx);

        ulong gasWanted = (ulong) Math.Ceiling((simulation.GasUsed * gasMultiplier) + gasOffset);
        var fee = await _gasFeeProvider!.GetFeeForGasAsync(gasWanted);

        return await PublishTxAsync(tx, fee, deadline, cancellationToken);
    }

    public TModule Module<TModule>() where TModule : IModule
    {
        AssertReady(false);
        return _provider.GetService<TModule>()
                ?? throw new InvalidOperationException("Module not installed!");
    }

    public IEnumerable<(Type, IModule)> GetAllModules()
    {
        AssertReady(false);
        foreach(var type in _moduleTypes)
        {
            yield return (type, (IModule) _provider.GetRequiredService(type));
        }
    }

    IInternalCosmClient ICosmClient.AsInternal()
        => this;
    public IInternalCosmTxClient AsInternal()
        => this;
}
