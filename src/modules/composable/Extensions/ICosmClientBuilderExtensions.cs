﻿using Cosm.Net.Client;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallComposable(this CosmClientBuilder builder, string bech32Prefix = "centauri")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(60))
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly());
}
