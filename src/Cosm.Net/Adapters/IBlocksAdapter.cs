using Cosm.Net.Models;
using Cosm.Net.Modules;
using Grpc.Core;

namespace Cosm.Net.Adapters;
public interface IBlocksAdapter : IModule
{
    public Task<Block> GetLatestAsync(Metadata? metadata = null, DateTime? deadline = null,  CancellationToken cancellationToken = default);
    public Task<Block> GetLatestAsync(CallOptions options);

    public Task<Block> GetByHeightAsync(ulong height, Metadata? metadata = null, DateTime? deadline = null, CancellationToken cancellationToken = default);
    public Task<Block> GetByHeightAsync(ulong height, CallOptions options);

    public async Task<DateTimeOffset> GetBlockTimestampAsync(
        ulong height, Metadata? metadata = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        => (await GetByHeightAsync(height, metadata, deadline, cancellationToken)).Timestamp;
    public async Task<DateTimeOffset> GetBlockTimestampAsync(ulong height, CallOptions options)
        => (await GetByHeightAsync(height, options)).Timestamp;

    public async Task<ulong> GetLatestHeightAsync(Metadata? metadata = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        => (await GetLatestAsync(metadata, deadline, cancellationToken)).Height;
    public async Task<ulong> GetLatestHeightAsync(CallOptions options)
        => (await GetLatestAsync(options)).Height;
}
