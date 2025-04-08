using Cosm.Net.Models;
using Cosm.Net.Modules;
using Grpc.Core;

namespace Cosm.Net.Adapters;
public interface IBlocksAdapter : IModule
{
    public Task<Block> GetLatestAsync(Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default);
    public Task<Block> GetLatestAsync(CallOptions options);

    public Task<Block> GetByHeightAsync(long height, Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default);
    public Task<Block> GetByHeightAsync(long height, CallOptions options);

    public async Task<DateTimeOffset> GetBlockTimestampAsync(
        long height, Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        => (await GetByHeightAsync(height, headers, deadline, cancellationToken)).Timestamp;
    public async Task<DateTimeOffset> GetBlockTimestampAsync(long height, CallOptions options)
        => (await GetByHeightAsync(height, options)).Timestamp;

    public async Task<long> GetLatestHeightAsync(Metadata? headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default)
        => (await GetLatestAsync(headers, deadline, cancellationToken)).Height;
    public async Task<long> GetLatestHeightAsync(CallOptions options)
        => (await GetLatestAsync(options)).Height;
}
