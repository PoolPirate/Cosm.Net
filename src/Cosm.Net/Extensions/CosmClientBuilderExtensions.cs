using Cosm.Net.Client;
using Cosm.Net.Wallet;

namespace Cosm.Net.Extensions;
public static class CosmClientBuilderExtensions
{
    public static CosmClientBuilder WithSigner(this CosmClientBuilder builder, IOfflineSigner signer)
    {

        return builder;
    }
}
