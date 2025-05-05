namespace Cosm.Net.Generators.Proto.Adapters;
public static class BlocksAdapter
{
    public const string Code =
        """
        #nullable enable

        using Cosm.Net.Modules;
        using Grpc.Core;
        using Cosm.Net.Models;

        namespace Cosm.Net.Adapters;

        internal class BlocksAdapter(ITendermintModule tendermintModule) : IBlocksAdapter
        {
            private Block ParseBlock(Tendermint.Types.Block tendermintBlock)
            {
                return new Block(
                    tendermintBlock.Header.Height,
                    tendermintBlock.Header.Time.ToDateTimeOffset(),
                    tendermintBlock.Header.ProposerAddress,
                    tendermintBlock.Data.Txs,
                    tendermintBlock.Header,
                    tendermintBlock.LastCommit
                );
            }
            
            public async Task<Block> GetLatestAsync(Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
            {
                var response = await tendermintModule.GetLatestBlockAsync(headers, deadline, cancellationToken);
                return ParseBlock(response.Block);
            }
            public async Task<Block> GetLatestAsync(CallOptions options)
            {
                var response = await tendermintModule.GetLatestBlockAsync(options);
                return ParseBlock(response.Block);
            }

            public async Task<Block> GetByHeightAsync(long height, Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
            {
                var response = await tendermintModule.GetBlockByHeightAsync((long) height, headers, deadline, cancellationToken);
                return ParseBlock(response.Block);
            }

            public async Task<Block> GetByHeightAsync(long height, CallOptions options)
            {
                var response = await tendermintModule.GetBlockByHeightAsync((long) height, options);
                return ParseBlock(response.Block);
            }
        }
        """;
}
