using Cosm.Net.Client;

namespace Cosm.Net.CosmosSdk;
public static class ICosmClientExtensions
{
    public static ITendermintModule Tendermint(this ICosmClient client)
        => client.Module<ITendermintModule>();
}
