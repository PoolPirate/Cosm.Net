using Cosm.Net.Client;

namespace Cosm.Net.Osmosis.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static CosmClientBuilder AddOsmosis(this CosmClientBuilder builder)
    {
        builder.RegisterModule<GammModule>();

        return builder;
    }
}
