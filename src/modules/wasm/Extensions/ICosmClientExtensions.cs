using Cosm.Net.Client;
using Cosm.Net.Wasm.Models;
using Cosm.Net.Wasm.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Wasm.Extensions;
public static class ICosmClientExtensions
{
    public static TContract Contract<TContract>(this ICosmClient client, string contractAddress)
        where TContract : IContract 
        => client.AsInternal().ServiceProvider.GetRequiredService<ContractSchemaStore>()
            .InstantiateContract<TContract>(contractAddress);
}
