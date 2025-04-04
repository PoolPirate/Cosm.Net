﻿using Cosm.Net.Adapters;
using Cosm.Net.Client;
using Cosm.Net.Modules;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallStargaze(this CosmClientBuilder builder, string bech32Prefix = "stars")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(50))
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly());
}
