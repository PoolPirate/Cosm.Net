using Cosm.Net.Models;
using Cosm.Net.Modules;
using Google.Protobuf;
using Grpc.Core;

namespace Cosm.Net.Adapters;
public interface ITxModuleAdapter : IModule
{
    public Task<TxSimulation> SimulateAsync(ByteString txBytes, Metadata? headers = default,
        DateTime? deadline = default, CancellationToken cancellationToken = default);

    public Task<TxSubmission> BroadcastTxAsync(ByteString txBytes, BroadcastMode mode,
        Metadata? headers = default, DateTime? deadline = default, CancellationToken cancellationToken = default);
}
