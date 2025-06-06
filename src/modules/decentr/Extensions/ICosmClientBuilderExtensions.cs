﻿using Cosm.Net.Client;
using System.Reflection;

namespace Cosm.Net.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder InstallDecentr(this CosmClientBuilder builder, string bech32Prefix = "decentr")
        => builder
            .AsInternal().UseCosmosTxStructure()
            .AsInternal().WithChainInfo(bech32Prefix, TimeSpan.FromSeconds(120))
            .AsInternal().RegisterModulesFromAssembly(Assembly.GetExecutingAssembly());
}
