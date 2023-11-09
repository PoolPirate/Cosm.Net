using Cosm.Net.Base;

namespace Cosm.Net.Osmosis.Extensions;
public static class ICosmClientBuilderExtensions
{
    public static TCosmClientBuilder AddOsmosis<TCosmClientBuilder>(this ICosmClientBuilder<TCosmClientBuilder> builder)
    where TCosmClientBuilder : class
    {
        builder.RegisterModule<GammModule>();

        return (TCosmClientBuilder) builder;
    }
}
