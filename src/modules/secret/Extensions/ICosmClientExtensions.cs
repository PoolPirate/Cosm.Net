using Cosm.Net.Client;
using Cosm.Net.Extensions;
using Cosm.Net.Models;
using Cosm.Net.Secret.Modules;

namespace Cosm.Net.Secret.Extensions;
public static class ICosmClientExtensions
{
    public static async Task<TContract> ContractWithCodeHashAsync<TContract>(this ICosmClient cosmClient, string contractAddress)
        where TContract : IContract
    {
        var computeModule = cosmClient.Module<IComputeModule>();
        var codeHashResponse = await computeModule.CodeHashByContractAddressAsync(contractAddress);

        return cosmClient.Contract<TContract>(contractAddress, codeHashResponse.CodeHash);
    }
}
