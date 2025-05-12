using Cosm.Net.Client;
using Cosm.Net.Models;
using Cosm.Net.Modules;
using Cosm.Net.Services;

namespace Cosm.Net.Extensions;
public static class ICosmClientExtensions
{
    /// <summary>
    /// Fetches the codehash for the given contractAddress and returns a contract with the CodeHash property populated.
    /// </summary>
    /// <remarks>
    /// On SecretNetwork contract interactions require the contract instance to have the CodeHash property populated.
    /// </remarks>
    /// <typeparam name="TContract"></typeparam>
    /// <param name="cosmClient"></param>
    /// <param name="contractAddress"></param>
    /// <returns></returns>
    public static async Task<TContract> ContractWithCodeHashAsync<TContract>(this ICosmClient cosmClient, string contractAddress)
        where TContract : IWasmContract
    {
        var computeModule = cosmClient.Module<IComputeModule>();
        var codeHashResponse = await computeModule.CodeHashByContractAddressAsync(contractAddress);

        return cosmClient.Contract<TContract>(contractAddress, codeHashResponse.CodeHash);
    }

    /// <summary>
    /// Creates a decryptor that enables decrypting encrypted transaction data from the secret network compute module.
    /// </summary>
    /// <param name="cosmClient"></param>
    /// <param name="context">The encryption context used by the transaction.</param>
    /// <returns></returns>
    public static IMessageDescryptor GetMessageDecryptor(this ICosmClient cosmClient, SecretEncryptionContext context)
        => new SecretMessageDecryptor(context.TxEncryptionKey);
}
