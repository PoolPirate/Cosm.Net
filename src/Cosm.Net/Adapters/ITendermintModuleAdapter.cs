using Cosm.Net.Modules;
using Grpc.Core;

namespace Cosm.Net.Adapters;
public interface ITendermintModuleAdapter : IModule
{
    public Task<string> GetChainId(Metadata? headers = default, DateTime? deadline = default, CancellationToken cancellationToken = default);
}
