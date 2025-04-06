namespace Cosm.Net.Generators.Proto.Adapters;
public class BlocksAdapter
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
            private readonly ITendermintModule _tendermintModule = tendermintModule;

            private Block ParseBlock(Tendermint.Types.Block tendermintBlock)
            {
                return new Block(
                    (ulong) tendermintBlock.Header.Height,
                    tendermintBlock.Header.Time.ToDateTimeOffset(),
                    tendermintBlock.Header.ProposerAddress,
                    tendermintBlock.Data.Txs,
                    tendermintBlock.Header,
                    tendermintBlock.LastCommit
                );
            }
            
            public async Task<Block> GetLatestAsync(Metadata? metadata = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
            {
                var response = await _tendermintModule.GetLatestBlockAsync(metadata, deadline, cancellationToken);
                return ParseBlock(response.Block);
            }
            public async Task<Block> GetLatestAsync(CallOptions options)
            {
                var response = await _tendermintModule.GetLatestBlockAsync(options);
                return ParseBlock(response.Block);
            }

            public async Task<Block> GetByHeightAsync(ulong height, Metadata? metadata = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
            {
                var response = await _tendermintModule.GetBlockByHeightAsync((long) height, metadata, deadline, cancellationToken);
                return ParseBlock(response.Block);
            }

            public async Task<Block> GetByHeightAsync(ulong height, CallOptions options)
            {
                var response = await _tendermintModule.GetBlockByHeightAsync((long) height, options);
                return ParseBlock(response.Block);
            }
        }
        """;
}
