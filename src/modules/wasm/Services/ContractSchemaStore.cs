﻿using Cosm.Net.Wasm.Models;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Math.EC.Multiplier;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cosm.Net.Wasm.Services;
public class ContractSchemaStore
{
    private IServiceProvider? _provider;
    private readonly Dictionary<Type, Func<IWasmModule, string, IContract>> _contractConstructors = [];

    public void InitProvider(IServiceProvider provider)
    {
        if (_provider is not null)
        {
            return;
        }

        _provider = provider;
    }

    public void RegisterContractSchema<TContract>()
        where TContract : IContract
    {
        if (_contractConstructors.ContainsKey(typeof(TContract)))
        {
            return;
        }

        var contractType = GetContractImplementationType(typeof(TContract))
            ?? throw new InvalidOperationException("Failed to register contract. Contract implementation not found. " +
            "Check that the source generator ran successfully!");

        var constructor = contractType.GetConstructor([
            typeof(IWasmModule),
            typeof(string)
        ]) ?? throw new InvalidOperationException("Failed to register contract. Contract implementation does not contain a valid constructor." +
            "Check that the source generator ran successfully!");

        _contractConstructors.Add(
            typeof(TContract), 
            (wasm, contractAddress) => (TContract) constructor.Invoke([wasm, contractAddress])
        );
    }

    public TContract InstantiateContract<TContract>(string contractAddress)
        where TContract : IContract
    {
        if (!_contractConstructors.TryGetValue(typeof(TContract), out var contractConstructor))
        {
            throw new InvalidOperationException($"Contract schema not registered: {typeof(TContract).Name}");
        }
        //
        return (TContract) contractConstructor.Invoke(
            GetProvider().GetRequiredService<IWasmModule>(),
            contractAddress
        );
    }

    private IServiceProvider GetProvider() 
        => _provider is null ? throw new NotSupportedException() : _provider;

    private static Type? GetContractImplementationType(Type interfaceType)
    => (Assembly.GetAssembly(interfaceType) ?? throw new InvalidOperationException("Failed to load contract Assembly"))
        .GetTypes()
        .Where(x => x.GetInterfaces()
            .Any(x => x == interfaceType))
        .FirstOrDefault();
}
