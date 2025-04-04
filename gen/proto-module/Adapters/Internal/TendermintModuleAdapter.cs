namespace Cosm.Net.Generators.Proto.Adapters.Internal;
public static class TendermintModuleAdapter
{
    public const string Code =
        """
        #nullable enable

        using Cosm.Net.Modules;
        using Grpc.Core;

        namespace Cosm.Net.Adapters.Internal;

        internal class TendermintModuleAdapter(ITendermintModule tendermintModule) : IInternalTendermintAdapter
        {
            public async Task<string> GetChainId(Metadata? headers, DateTime? deadline, CancellationToken cancellationToken)
            {
                var nodeInfo = await tendermintModule.GetNodeInfoAsync(headers, deadline, cancellationToken);
                return nodeInfo.DefaultNodeInfo.Network;
            }
        }
        """;
}
