using Cosm.Net.Client;
using Cosm.Net.Wasm.Models;

namespace Cosm.Net.Wasm.Extensions;
public static class ICosmClientExtensions
{
    public static TContract Contract<TContract>(this ICosmClient client)
        where TContract : IContract
    {
        throw new NotImplementedException();
    }
}
