using Cosm.Net.Client;

namespace Cosm.Net.CosmosSdk;
public static class ICosmClientExtensions
{
    public static IABCIService ABCIService(this ICosmClient client)
        => client.Module<IABCIService>();

    public static INodeService Node(this ICosmClient client)
        => client.Module<INodeService>();
}
