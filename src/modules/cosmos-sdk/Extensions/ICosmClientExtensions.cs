using Cosm.Net.Client;

namespace Cosm.Net.CosmosSdk;
public static class ICosmClientExtensions
{
    public static ITendermintService Tendermint(this ICosmClient client)
        => client.Module<ITendermintService>();
}
