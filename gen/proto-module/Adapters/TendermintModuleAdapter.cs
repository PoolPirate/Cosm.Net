namespace Cosm.Net.Generators.Proto.Adapters;
public static class TendermintModuleAdapter
{
    public const string Code =
        """
        #nullable enable

        using Cosm.Net.Modules;
        using Grpc.Core;

        namespace Cosm.Net.Adapters;

        internal class TendermintModuleAdapter(ITendermintModule tendermintModule) : ITendermintModuleAdapter
        {
            async Task<string> ITendermintModuleAdapter.GetChainId(Metadata? headers, DateTime? deadline, CancellationToken cancellationToken)
            {
                var nodeInfo = await tendermintModule.GetNodeInfoAsync(headers, deadline, cancellationToken);
                return nodeInfo.DefaultNodeInfo.Network;
            }
        }
        """;
}
