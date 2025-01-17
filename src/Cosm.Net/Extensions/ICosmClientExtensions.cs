using Cosm.Net.Client;
using Cosm.Net.Models;
using Cosm.Net.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cosm.Net.Extensions;
public static class ICosmClientExtensions
{
    public static TContract Contract<TContract>(this ICosmClient client, string contractAddress, string? codeHash = null)
        where TContract : IContract
        => client.AsInternal().ServiceProvider.GetRequiredService<IContractFactory>()
            .Create<TContract>(contractAddress, codeHash);
}
